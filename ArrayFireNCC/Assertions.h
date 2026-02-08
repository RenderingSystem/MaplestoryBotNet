#pragma once
#include <cassert>


namespace ArrayFireNCCTests {
    using namespace System;
    using namespace System::Reflection;
    using namespace System::ComponentModel;
    using namespace System::Diagnostics;


    public ref class UnitTestAssertions {

    public:

        void Assert(bool condition);

        void AssertBytesEqual(array<Byte>^ a1, array<Byte>^ a2);

    };

}