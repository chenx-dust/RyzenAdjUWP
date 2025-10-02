using RyzenAdjUWP.Backend.Libraries;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RyzenAdjUWP.Backend
{
    public enum RyzenAdjError
    {
        None = 0,
        FamilyUnsupported = RyzenAdjNative.ADJ_ERR_FAM_UNSUPPORTED, // -1
        SmuTimeout = RyzenAdjNative.ADJ_ERR_SMU_TIMEOUT,      // -2
        SmuUnsupported = RyzenAdjNative.ADJ_ERR_SMU_UNSUPPORTED,  // -3
        SmuRejected = RyzenAdjNative.ADJ_ERR_SMU_REJECTED,     // -4
        MemoryAccess = RyzenAdjNative.ADJ_ERR_MEMORY_ACCESS,    // -5
        UnknownNegative = int.MinValue
    }

    public sealed class RyzenAdjException : Exception
    {
        public RyzenAdjError Error { get; }
        public int RawCode { get; }
        public RyzenAdjException(RyzenAdjError error, int raw, string message = null)
            : base(message ?? $"RyzenAdj error: {error} (code {raw})")
        {
            Error = error;
            RawCode = raw;
        }

        public static RyzenAdjError FromReturnCode(int rc)
        {
            if (Enum.IsDefined(typeof(RyzenAdjError), rc))
                return (RyzenAdjError)rc;
            else if (rc < 0)
                return RyzenAdjError.UnknownNegative;
            else
                return RyzenAdjError.None;
        }

        public static void ThrowIfError(int rc, [CallerMemberName] string api = null)
        {
            if (rc >= 0) return;
            var err = FromReturnCode(rc);
            throw new RyzenAdjException(err, rc, $"Call '{api}' failed: {err} ({rc})");
        }
    }

    public sealed class RyzenAdj : IDisposable
    {
        private readonly object _lock = new object();
        private IntPtr _handle;
        private bool _disposed;

        public ryzen_family Family { get; }
        public int BiosIfVersion { get; }

        private RyzenAdj(IntPtr handle)
        {
            _handle = handle;
            Family = RyzenAdjNative.get_cpu_family(handle);
            BiosIfVersion = RyzenAdjNative.get_bios_if_ver(handle);
        }

        public static RyzenAdj Open(bool initTable = true)
        {
            var h = RyzenAdjNative.init_ryzenadj();
            if (h == IntPtr.Zero)
                throw new RyzenAdjException(RyzenAdjError.MemoryAccess, (int)RyzenAdjError.UnknownNegative, "init_ryzenadj returned null.");

            try
            {
                var fam = RyzenAdjNative.get_cpu_family(h);
                if (fam == ryzen_family.FAM_UNKNOWN || fam == ryzen_family.FAM_END)
                {
                    RyzenAdjNative.cleanup_ryzenadj(h);
                    throw new RyzenAdjException(RyzenAdjError.FamilyUnsupported, RyzenAdjNative.ADJ_ERR_FAM_UNSUPPORTED, $"Unsupported CPU family: {fam}");
                }

                var client = new RyzenAdj(h);
                if (initTable)
                {
                    var rc = RyzenAdjNative.init_table(h);
                    RyzenAdjException.ThrowIfError(rc);
                }
                return client;
            }
            catch
            {
                try { RyzenAdjNative.cleanup_ryzenadj(h); } catch { /* ignored */ }
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            lock (_lock)
            {
                if (_disposed) return;
                if (_handle != IntPtr.Zero)
                {
                    RyzenAdjNative.cleanup_ryzenadj(_handle);
                    _handle = IntPtr.Zero;
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~RyzenAdj() { Dispose(); }

        private void EnsureNotDisposed()
        {
            if (_disposed || _handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(RyzenAdj));
        }

        public void RefreshTable()
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                var rc = RyzenAdjNative.refresh_table(_handle);
                RyzenAdjException.ThrowIfError(rc);
            }
        }

        #region Properties
        public float StapmLimit_W
        {
            get => WithLock(() => RyzenAdjNative.get_stapm_limit(_handle));
            set => Call(() => RyzenAdjNative.set_stapm_limit(_handle, checked((uint)Math.Round(value * 1000))));
        }

        public float FastLimit_W
        {
            get => WithLock(() => RyzenAdjNative.get_fast_limit(_handle));
            set => Call(() => RyzenAdjNative.set_fast_limit(_handle, checked((uint)Math.Round(value * 1000))));
        }

        public float SlowLimit_W
        {
            get => WithLock(() => RyzenAdjNative.get_slow_limit(_handle));
            set => Call(() => RyzenAdjNative.set_slow_limit(_handle, checked((uint)Math.Round(value * 1000))));
        }

        public float StapmTime_s
        {
            get => WithLock(() => RyzenAdjNative.get_stapm_time(_handle));
            set => Call(() => RyzenAdjNative.set_stapm_time(_handle, checked((uint)Math.Round(value))));
        }

        public float SlowTime_s
        {
            get => WithLock(() => RyzenAdjNative.get_slow_time(_handle));
            set => Call(() => RyzenAdjNative.set_slow_time(_handle, checked((uint)Math.Round(value))));
        }

        public float TctlTemp_C
        {
            get => WithLock(() => RyzenAdjNative.get_tctl_temp(_handle));
            set => Call(() => RyzenAdjNative.set_tctl_temp(_handle, checked((uint)Math.Round(value))));
        }

        public float MinGfxClk_MHz
        {
            set => Call(() => RyzenAdjNative.set_min_gfxclk_freq(_handle, checked((uint)Math.Round(value))));
        }

        public float MaxGfxClk_MHz
        {
            set => Call(() => RyzenAdjNative.set_max_gfxclk_freq(_handle, checked((uint)Math.Round(value))));
        }

        public float StapmValue_W => WithLock(() => RyzenAdjNative.get_stapm_value(_handle));
        public float FastValue_W => WithLock(() => RyzenAdjNative.get_fast_value(_handle));
        public float SlowValue_W => WithLock(() => RyzenAdjNative.get_slow_value(_handle));
        public float GfxVolt_V => WithLock(() => RyzenAdjNative.get_gfx_volt(_handle));
        public float SocketPower_W => WithLock(() => RyzenAdjNative.get_socket_power(_handle));

        #endregion

        #region Helper
        private void Call(Func<int> native)
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                int rc = native();
                RyzenAdjException.ThrowIfError(rc);
            }
        }

        private T WithLock<T>(Func<T> getter)
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                return getter();
            }
        }
        #endregion
    }
}
