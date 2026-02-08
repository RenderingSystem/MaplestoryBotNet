#pragma once
#ifdef max
#undef max
#endif
#ifdef min
#undef min
#endif
#pragma managed(push, off)
#pragma warning(push)
#pragma warning(disable: 4275)
#pragma warning(disable: 4251) 
#include <arrayfire.h>
#pragma warning(pop)
#pragma managed(pop)
#ifdef max
#undef max
#endif
#ifdef min
#undef min
#endif