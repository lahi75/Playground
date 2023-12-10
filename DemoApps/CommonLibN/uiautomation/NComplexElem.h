// ----------------------------------------------------------------------------

#pragma once

#include <Windows.h>
#include "NSimpleElem.h"

// ----------------------------------------------------------------------------


namespace Native
{
	namespace UIAutomation
	{
		struct NComplexElem : NSimpleElem
		{
			NComplexElem()
			{
				hasWritableText = false;
				className = NULL;
				windowHandle = NULL;
			}

			virtual ~NComplexElem()
			{
				Clear();		
			}

			//Clear allocated resources of element
			void Clear()
			{
				//call base function
				NSimpleElem::Clear();

				if(className != NULL)
				{
					SysFreeString(className);
					className = NULL;
				}
			}

			//Clone element
			void Clone(NComplexElem& targetElement)
			{
				//call base function
				NSimpleElem::Clone(targetElement);
			
				targetElement.hasWritableText = this->hasWritableText;

				if(this->className != NULL)
					targetElement.className = SysAllocString(this->className);

				targetElement.windowHandle = this->windowHandle;
			}

			bool operator==(const NComplexElem& e)
			{		
				if(&e == NULL)
					return false;

				return (NSimpleElem::operator==(e) && wcscmp(className, e.className) == 0 && hasWritableText == e.hasWritableText && windowHandle == e.windowHandle);			
			}

			bool operator!=(const NComplexElem& e)
			{			
				return !operator==(e);
			}

			bool hasWritableText;

			BSTR className;

			UIA_HWND windowHandle;
		};
	}
}

// ----------------------------------------------------------------------------