#include "pch.h"
#include "AcceleratedDeviceSelectionSystem.h"


namespace ArrayFireNCC {

    void multiply_time_test(void)
    {
        af::array a = af::randu(1024, 1024);
        af::array b = af::randu(1024, 1024);
        af::array c = af::matmul(a, b);
        af::eval(c);
    }

    void ArrayFireDeviceSelector::_set_best_backend(void)
    {
        auto backends = af::getAvailableBackends();
        auto backend(
            (backends & AF_BACKEND_CUDA) ? AF_BACKEND_CUDA :
            (backends & AF_BACKEND_ONEAPI) ? AF_BACKEND_ONEAPI :
            (backends & AF_BACKEND_OPENCL) ? AF_BACKEND_OPENCL :
            AF_BACKEND_CPU
        );
        af::setBackend(backend);
    }

    void ArrayFireDeviceSelector::_print_device_info(double time)
    {
        std::vector<char> device_name(1000);
        std::vector<char> _1(1000);
        std::vector<char> _2(1000);
        std::vector<char> _3(1000);
        af::deviceInfo(device_name.data(), _1.data(), _2.data(), _3.data());
        Console::Out->WriteLine(
            gcnew String(device_name.data())
            + " time - " + time.ToString()
        );
    }

    void ArrayFireDeviceSelector::_set_best_backend_device(void)
    {
        int best_device = 0;
        int device_count = af::getDeviceCount();
        float best_performance = 1000000.0;
        if (device_count > 1)
        {
            for (int device = 0; device < device_count; device++)
            {
                af::setDevice(device);
                double time = af::timeit(multiply_time_test);
                float current_performance = static_cast<float>(time);
                if (current_performance < best_performance)
                {
                    best_performance = current_performance;
                    best_device = device;
                }
            }
        }
        else
        {
            af::setDevice(best_device);
        }
    }

    void ArrayFireDeviceSelector::select(void) {
        _set_best_backend();
        _set_best_backend_device();
    }

}


namespace ArrayFireNCC {

    ArrayFireDeviceSelectionSystem::ArrayFireDeviceSelectionSystem(
        AbstractAcceleratedDeviceSelector^ selector
    )
    {
        _selector = selector;
    }

    void ArrayFireDeviceSelectionSystem::context_select(void)
    {
        if (backend == AF_BACKEND_DEFAULT || backend_device < 0)
        {
            _selector->select();
            backend = af::getActiveBackend();
            backend_device = af::getDevice();
        }
        else {
            af::setBackend(backend);
            af::setDevice(backend_device);
        }
    }

    int ArrayFireDeviceSelectionSystem::context_selected()
    {
        return backend_device;
    }

}


namespace ArrayFireNCC {

    ArrayFireDeviceSelectionSystemFacade::ArrayFireDeviceSelectionSystemFacade(void)
    {
        if (!_current_system)
        {
            _current_system = gcnew ArrayFireDeviceSelectionSystem(
                gcnew ArrayFireDeviceSelector()
            );
        }
    }

    void ArrayFireDeviceSelectionSystemFacade::context_select(void) {
        _current_system->context_select();
    }

    int ArrayFireDeviceSelectionSystemFacade::context_selected()
    {
        return _current_system->context_selected();
    }

}
