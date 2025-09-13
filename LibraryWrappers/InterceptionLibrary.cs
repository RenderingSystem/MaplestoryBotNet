using Interception;


namespace MaplestoryBotNet.LibraryWrappers
{
    public unsafe abstract class AbstractInterceptionLibrary
    {
        const int INFINITE = unchecked((int)0xFFFFFFFF);

        const int MaxKeyboards = 10;

        const int MaxMouses = 10;

        public abstract nint CreateContext();

        public abstract void DestroyContext(Context context);

        public abstract Precedence GetPrecedence(Context context, Device device);

        public abstract void SetPrecedence(Context context, Device device, Precedence precedence);

        public abstract InterceptionInterop.Filter GetFilter(Context context, Device device);

        public abstract void SetFilter(Context context, InterceptionInterop.Predicate interception_predicate, InterceptionInterop.Filter filter);

        public Device Wait(Context context) => WaitWithTimeout(context, INFINITE);

        public abstract Device WaitWithTimeout(Context context, int milliseconds);

        public abstract int Send(Context context, Device device, InterceptionInterop.Stroke* stroke, int nstroke);

        public abstract int Receive(Context context, Device device, InterceptionInterop.Stroke* stroke, uint nstroke);

        public abstract int GetHardwareId(Context context, Device device, void* hardware_id_buffer, uint buffer_size);

        public bool IsValid(Device device) => !IsKeyboard(device) && !IsMouse(device);

        int INTERCEPTION_KEYBOARD(int index) => index + 1;

        public bool IsKeyboard(Device device) => device >= INTERCEPTION_KEYBOARD(0) && device <= INTERCEPTION_KEYBOARD(MaxKeyboards - 1);

        int INTERCEPTION_MOUSE(int index) => MaxKeyboards + index + 1;

        public bool IsMouse(Device device) => device >= INTERCEPTION_MOUSE(0) && device <= INTERCEPTION_MOUSE(MaxMouses - 1);
    }


    public class InterceptionLibrary : AbstractInterceptionLibrary
    {
        public override nint CreateContext()
        {
            return InterceptionInterop.interception_create_context();
        }

        public override void DestroyContext(nint context)
        {
            InterceptionInterop.interception_destroy_context(context);
        }

        public override InterceptionInterop.Filter GetFilter(nint context, int device)
        {
            return InterceptionInterop.interception_get_filter(context, device);
        }

        public override unsafe int GetHardwareId(nint context, int device, void* hardware_id_buffer, uint buffer_size)
        {
            return InterceptionInterop.interception_get_hardware_id(context, device, hardware_id_buffer, buffer_size);
        }

        public override int GetPrecedence(nint context, int device)
        {
            return InterceptionInterop.interception_get_precedence(context, device);
        }

        public override unsafe int Receive(nint context, int device, InterceptionInterop.Stroke* stroke, uint nstroke)
        {
            return InterceptionInterop.interception_receive(context, device, stroke, nstroke);
        }

        public override unsafe int Send(nint context, int device, InterceptionInterop.Stroke* stroke, int nstroke)
        {
            return InterceptionInterop.interception_send(context, device, stroke, nstroke);
        }

        public override void SetFilter(nint context, InterceptionInterop.Predicate interception_predicate, InterceptionInterop.Filter filter)
        {
            InterceptionInterop.interception_set_filter(context, interception_predicate, filter);
        }

        public override void SetPrecedence(nint context, int device, int precedence)
        {
            InterceptionInterop.interception_set_precedence(context, device, precedence);
        }

        public override int WaitWithTimeout(nint context, int milliseconds)
        {
            return InterceptionInterop.interception_wait_with_timeout(context, milliseconds);
        }
    }
}
