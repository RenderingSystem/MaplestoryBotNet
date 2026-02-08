#include "pch.h"
#include "AcceleratedDeviceSelectionSystemMocks.h"


namespace ArrayFireNCCTests {

    void ArrayFireDeviceSelectionSystemMock::context_select(void) {
        context_select_calls++;
        call_order->Add("ArrayFireDeviceSelectionSystemMock::context_select");
    }

    int ArrayFireDeviceSelectionSystemMock::context_selected()
    {
        context_selected_calls++;
        call_order->Add("ArrayFireDeviceSelectionSystemMock::context_selected");
        if (context_selected_index < (int)context_selected_return->Count)
            return context_selected_return[context_selected_index++];
        throw gcnew IndexOutOfRangeException();
    }

}


namespace ArrayFireNCCTests {

    void AcceleratedDeviceSelectionSystemMock::context_select(void) {
        context_select_calls++;
        call_order->Add("AcceleratedDeviceSelectionSystemMock::context_select");
    }

    int AcceleratedDeviceSelectionSystemMock::context_selected()
    {
        context_selected_calls++;
        call_order->Add("AcceleratedDeviceSelectionSystemMock::context_selected");
        if (context_selected_index < (int)context_selected_return->Count)
            return context_selected_return[context_selected_index++];
        throw gcnew IndexOutOfRangeException();
    }

}
