// ----------------------------------------------------------------------------

#pragma once

#include <Windows.h>

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		struct NElem
		{
			NElem()
			{
				name = NULL;
				controlType = 0;
				bounds = RECT();
			}

			virtual ~NElem()
			{
				Clear();		
			}

			//Clear allocated resources of element
			void Clear()
			{
				if(name != NULL)
				{
					SysFreeString(name);
					name = NULL;
				}
			}

			//Clone element
			void Clone(NElem& targetElement)
			{
				targetElement.Clear();

				if(this->name != NULL)
					targetElement.name = SysAllocString(this->name);

				targetElement.bounds = this->bounds;
				targetElement.controlType = this->controlType;
			}

			bool operator==(const NElem& e)
			{			
				if(&e == NULL)
					return false;
			
				return (wcscmp(name, e.name) == 0 && controlType == e.controlType && bounds.bottom == e.bounds.bottom && bounds.top == e.bounds.top && bounds.left == e.bounds.left && bounds.right == e.bounds.right);
			}

			bool operator!=(const NElem& e)
			{			
				return !operator==(e);
			}

			BSTR name;
			int controlType;
			RECT bounds;
		};
	}
}

// ----------------------------------------------------------------------------