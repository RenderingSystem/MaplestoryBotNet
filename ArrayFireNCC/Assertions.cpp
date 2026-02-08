#include "pch.h"
#include "Assertions.h"


namespace ArrayFireNCCTests {

    void UnitTestAssertions::Assert(bool condition) {
        Debug::Assert(condition);
    }

    void UnitTestAssertions::AssertBytesEqual(array<Byte>^ a1, array<Byte>^ a2) {
        Debug::Assert(a1->Length == a2->Length);
        for (int i = 0; i < a1->Length; i++)
            Debug::Assert(a1[i] == a2[i]);
    }

}
