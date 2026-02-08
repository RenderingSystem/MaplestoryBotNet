#pragma once
#include "ArrayFireWrapper.h"
#include <thread>


namespace ArrayFireNCC {
    using namespace System;
    using namespace System::Collections::Generic;

    /**
     * @brief Abstract interface for a strategy that selects an accelerated computing device.
     *
     * This class defines the contract for any device selection algorithm. Concrete implementations
     * (like @ref ArrayFireDeviceSelector) will contain the specific logic for benchmarking and
     * choosing the best available hardware backend (e.g., CUDA, OpenCL, CPU) and device.
     * The strategy pattern allows the selection algorithm to be independent of the system that uses it.
     */
    public ref class AbstractAcceleratedDeviceSelector abstract {

    public:

        /**
         * @brief Executes the device selection algorithm.
         *
         * Implementations should benchmark available hardware and set the optimal backend
         * and device for subsequent accelerated computations.
         */
        virtual void select(void) abstract;

    };


    /**
     * @brief Abstract base class for a system that manages the context for accelerated device
     * selection.
     *
     * This system is responsible for ensuring the correct computational backend and device are active
     * for the current thread or context. It may perform an initial selection or simply restore a
     * previously selected optimal configuration.
     */
    public ref class AbstractAcceleratedDeviceSelectionSystem abstract {

    protected:

        /// @brief Static reference to the currently active device selection system.
        static AbstractAcceleratedDeviceSelectionSystem^ _current_system;

    public:

        /**
         * @brief Ensures the optimal accelerated device is selected and active for the current
         * context.
         *
         * This method should be called before any accelerated computations to guarantee they run on
         * the best available hardware. It typically caches the selection result for performance.
         */
        virtual void context_select(void) abstract;

        /**
         * @brief Gets the identifier of the currently selected accelerated device/context.
         *
         * @return An integer representing the unique identifier of the active computational
         * backend or device. Returns -1 if no context has been selected or the selection is invalid.
         */
        virtual int context_selected() abstract;

    };


    /**
     * @brief A performance test function used to benchmark different computational devices.
     *
     * This function performs a matrix multiplication, a common operation that is highly dependent
     on GPU/accelerator performance. The execution time of this function is measured by device
     * selectors to determine the fastest available hardware.
     */
    void multiply_time_test(void);


    /**
     * @brief Concrete device selector that uses the ArrayFire library to choose the best backend
     * and device.
     *
     * This class implements the @ref AbstractAcceleratedDeviceSelector interface. It benchmarks all
     * available ArrayFire backends (CUDA, OneAPI, OpenCL, CPU) and devices, selecting the one that
     * performs the @ref multiply_time_test the fastest. The results are printed to the console.
     */
    public ref class ArrayFireDeviceSelector
        : public AbstractAcceleratedDeviceSelector {

    private:

        /**
         * @brief Sets the most capable available ArrayFire backend.
         *
         * The selection is based on a priority order: CUDA is preferred for NVIDIA GPUs, followed by
         * OneAPI for Intel GPUs, then OpenCL for other GPUs/accelerators, with CPU as the fallback.
         */
        void _set_best_backend(void);

        /**
         * @brief Prints the name of the current device and the benchmark time to the console.
         *
         * @param[in] time The execution time (in seconds) of the benchmark on the current device.
         */
        void _print_device_info(double time);

        /**
         * @brief Benchmarks all devices on the current backend and selects the fastest one.
         *
         * Iterates through all available devices, runs a benchmark on each, and selects the device
         * with the lowest execution time. If only one device is available, it is selected and
         * benchmarked.
         */
        void _set_best_backend_device(void);

    public:

        /**
         * @brief Executes the full device selection process.
         *
         * This is the public interface method. It first selects the best backend, then the best device
         * within that backend, making the final selection the active device for ArrayFire operations.
         */
        virtual void select(void) override;


    };


    /**
     * @brief A system that manages the ArrayFire device context using a specific selection strategy.
     *
     * This class implements the @ref AbstractAcceleratedDeviceSelectionSystem. It uses a provided
     * @ref AbstractAcceleratedDeviceSelector to perform an initial device selection. The results of
     * this selection are cached. Subsequent calls to @ref context_select will quickly restore the
     * cached optimal backend and device, avoiding the overhead of re-benchmarking.
     */
    public ref class ArrayFireDeviceSelectionSystem
        : public AbstractAcceleratedDeviceSelectionSystem {

    private:

        /// @brief The strategy used to perform the initial device selection.
        AbstractAcceleratedDeviceSelector^ _selector;
        /// @brief The cached optimal backend chosen by the selector.
        af_backend backend = af_backend::AF_BACKEND_DEFAULT;
        /// @brief The cached optimal device index chosen by the selector.
        int backend_device = -1;

    public:

        /**
         * @brief Constructs a new system that will use the provided selector.
         *
         * @param[in] selector The device selection strategy to use for the initial benchmark.
         */
        ArrayFireDeviceSelectionSystem(AbstractAcceleratedDeviceSelector^ selector);

        /**
         * @brief Ensures the optimal ArrayFire device is active.
         *
         * If no selection has been cached, it runs the full selection algorithm.
         * If a selection is cached, it efficiently restores that backend and device.
         */
        virtual void context_select(void) override;

        /**
         * @brief Returns the device index of the currently selected ArrayFire device.
         *
         * This method returns the device index that was determined to be optimal by the initial
         * selection (via the provided selector) and cached. If no selection has been made (i.e.,
         * @ref context_select() has not been successfully called), it returns -1.
         *
         * @return The optimal device index (non-negative) if a selection has been made, otherwise 0.
         */
        virtual int context_selected() override;

    };


    /**
     * @brief A facade that provides a simple, global interface to the device selection system.
     *
     * This class implements the Singleton and Facade patterns. It ensures there is only one
     * global instance of the device selection system (@ref _current_system) and provides a
     * straightforward static method (@ref context_select) to access its functionality.
     * It constructs the system with a default @ref ArrayFireDeviceSelector.
     */
    public ref class ArrayFireDeviceSelectionSystemFacade
        : public AbstractAcceleratedDeviceSelectionSystem {

    public:

        /**
         * @brief Initializes the facade and ensures the global system instance is created.
         *
         * If the global system doesn't exist, it creates an @ref ArrayFireDeviceSelectionSystem
         * configured with an @ref ArrayFireDeviceSelector.
         */
        ArrayFireDeviceSelectionSystemFacade(void);

        /**
         * @brief The simple, global entry point for ensuring the optimal device is selected.
         *
         * This method delegates the call to the underlying global system instance's
         * @ref ArrayFireDeviceSelectionSystem::context_select "context_select" method.
         */
        virtual void context_select(void) override;

        /**
         * @brief Returns the cached optimal device index from the global system's last selection.
         *
         * This method delegates to the global instance of `ArrayFireDeviceSelectionSystem` and returns
         * the device index stored during its last successful device selection.
         *
         * @return The device index (≥0) from the last successful device selection by the global system.
         *  Returns -1 if the global system has not performed a device selection or if the selection failed.
         */
        virtual int context_selected() override;

    };

}
