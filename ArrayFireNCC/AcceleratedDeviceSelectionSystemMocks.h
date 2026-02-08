#pragma once
#include "AcceleratedDeviceSelectionSystem.h"


namespace ArrayFireNCCTests {
    using namespace System::Collections::Generic;
    using namespace System;
    using namespace ArrayFireNCC;


    public ref class ArrayFireDeviceSelectionSystemMock : public AbstractAcceleratedDeviceSelectionSystem {

    public:

        List<String^>^ call_order = gcnew List<String^>();

        int context_select_calls = 0;
        virtual void context_select(void) override;

        int context_selected_calls = 0;
        int context_selected_index = 0;
        List<int>^ context_selected_return = gcnew List<int>();
        virtual int context_selected() override;

    };

    public ref class AcceleratedDeviceSelectionSystemMock : public AbstractAcceleratedDeviceSelectionSystem {

    public:

        List<String^>^ call_order = gcnew List<String^>();

        int context_select_calls = 0;
        virtual void context_select(void) override;

        int context_selected_calls = 0;
        int context_selected_index = 0;
        List<int>^ context_selected_return = gcnew List<int>();
        virtual int context_selected() override;
    };

}
