﻿
// ----------------------------------------------------------------------------

using System;
using System.Drawing;

// ----------------------------------------------------------------------------

namespace Phoebit.Desktop
{
    /// <summary>
    /// class defining an AOI object on the desktop
    /// </summary>
    [Serializable]
    public class Aoi
    {
        public enum AoiTypeName
        {
            Button	        =	50000,
            Calendar        =	50001,
            CheckBox	    =	50002,
            ComboBox	    =	50003,
            EditBox	        =	50004,
            Hyperlink	    =	50005,
            Image	        =	50006,
            ListItem	    =	50007,
            List	        =	50008,
            Menu	        =	50009,
            MenuBar	        =	50010,
            MenuItem	    =	50011,
            ProgressBar	    =	50012,
            RadioButton	    =	50013,
            ScrollBar	    =	50014,
            Slider	        =	50015,
            Spinner	        =	50016,
            StatusBar	    =	50017,
            TabControl	    =	50018,
            TabItem	        =	50019,
            Text	        =	50020,
            ToolBar	        =	50021,
            ToolTip         =	50022,
            Tree    	    =	50023,
            TreeItem    	=	50024,
            Custom	        =	50025,
            Group       	=	50026,
            Thumb       	=	50027,
            DataGrid    	=	50028,
            DataItem    	=	50029,
            Document	    =	50030,
            SplitButton 	=	50031,
            Window	        =	50032,
            Pane	        =	50033,
            Header	        =	50034,
            HeaderItem	    =	50035,
            Table	        =	50036,
            TitleBar	    =	50037,
            Separator	    =	50038,
            SemanticZoom    =   50039
        }

        /// <summary>
        /// ctor
        /// </summary>        
        public Aoi(long aoiType, Rectangle bounds, String name)
        {
            AoiType = aoiType;
            Bounds = bounds;
            Name = name;            
        }

        /// <summary>
        /// the AoiType ( IUIAutomationElement )
        /// </summary>
        public long AoiType
        {
            get;
            set;
        }

        /// <summary>
        /// return the name of the AOI type
        /// </summary>
        public AoiTypeName GetAoiTypeName()
        {            
            return (AoiTypeName)AoiType;            
        }

        /// <summary>
        /// bounding box of the AOI element
        /// </summary>
        public Rectangle Bounds
        {
            get;
            set;
        }
        
        /// <summary>
        /// name of the aoi
        /// </summary>
        public String Name
        {
            get;
            set;
        }      

        /// <summary>
        /// hashcode of the aoi
        /// </summary>        
        public override int GetHashCode()
        {
            return (AoiType.GetHashCode() + Bounds.GetHashCode() + Name.GetHashCode());
        }
        
        /// <summary>
        /// compare function
        /// </summary>        
        public override bool Equals(object obj)
        {
            Aoi aoi = (Aoi)obj;
            
            if(AoiType != aoi.AoiType)
                return false;
            if(Bounds != aoi.Bounds)
                return false;
            if(Name != aoi.Name)
                return false;            

            return true;
        }

        /// <summary>
        /// equal operator
        /// </summary>        
        public static bool operator ==(Aoi a1, Aoi a2)
        {
            return (a1.Equals(a2));
        }

        /// <summary>
        /// not equal operator
        /// </summary>        
        public static bool operator !=(Aoi a1, Aoi a2)
        {
            return (!a1.Equals(a2));
        }
         
    }
}

// ----------------------------------------------------------------------------