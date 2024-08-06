using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DualDeckEditorAddin
{
    public static class ControlMappings
    {
        public static Dictionary<string, string> TextBoxDimensionMappings { get; set; }
        public static Dictionary<string, string> TextBoxMappings { get; set; }
        public static Dictionary<string, string> CheckBoxMappings { get; set; }

        public static Dictionary<string, string> TextBoxDimensionMappingsMirror { get; private set; }
        public static Dictionary<string, string> TextBoxMappingsMirror { get; private set; }
        public static Dictionary<string, string> CheckBoxMappingsMirror { get; private set; }

        public static Dictionary<string, string> TextBoxDimensionMappingsAdjust { get; private set; }
        public static Dictionary<string, string> TextBoxMappingsAdjust { get; private set; }
        public static Dictionary<string, string> CheckBoxMappingsAdjust { get; private set; }

        public static Dictionary<string, string> TextBoxDimensionMappingsSkew { get; private set; }
        public static Dictionary<string, string> TextBoxMappingsSkew { get; private set; }
        public static Dictionary<string, string> CheckBoxMappingsSkew { get; private set; }

        public static Dictionary<string, string> TextBoxDimensionMappings2Skew { get; private set; }
        public static Dictionary<string, string> TextBoxMappings2Skew { get; private set; }
        public static Dictionary<string, string> CheckBoxMappings2Skew { get; private set; }

        public static Dictionary<string, string> mirroringPairsWithoutOffset { get; private set; }
        public static Dictionary<string, string> mirroringPairsWithOffset { get; private set; }

        static ControlMappings()
        {
            InitializeTextBoxMappingsMirror();
            InitializeCheckBoxMappingsMirror();
            InitializeTextBoxDimensionMappingsMirror();
            InitializeTextBoxMappingsAdjust();
            InitializeCheckBoxMappingsAdjust();
            InitializeTextBoxDimensionMappingsAdjust();
            InitializeTextBoxMappingsSkew();
            InitializeCheckBoxMappingsSkew();
            InitializeTextBoxDimensionMappingsSkew();
            InitializeTextBoxMappings2Skew();
            InitializeCheckBoxMappings2Skew();
            InitializeTextBoxDimensionMappings2Skew();
            InitializeMirroringPairs();
        }

        private static void InitializeTextBoxDimensionMappingsMirror()
        {
            TextBoxDimensionMappingsMirror = new Dictionary<string, string>
            {
                {"textBoxDD_Depth", "DD_Depth"},
                {"textBoxDD_Length", "DD_Length"},
                {"textBoxDD_Width", "DD_Width"},
                {"textBoxDD_BotJoint", "Bot_Joint_Size"},
                {"textBoxDD_LedgeJoint", "Ledge+Joint"}
            };
        }

        private static void InitializeTextBoxMappingsMirror()
        {
            TextBoxMappingsMirror = new Dictionary<string, string>
            {
                {"textBoxShort_A", "TrussA/B_Short"},
                {"textBoxLong_A", "TrussA/B_Long"},
                {"textBoxShort_01", "Truss01/11_Short"},
                {"textBoxLong_01", "Truss01/11_Long"},
                {"textBoxShort_02", "Truss02/10_Short"},
                {"textBoxLong_02", "Truss02/10_Long"},
                {"textBoxShort_03", "Truss03/09_Short"},
                {"textBoxLong_03", "Truss03/09_Long"},
                {"textBoxShort_04", "Truss04/08_Short"},
                {"textBoxLong_04", "Truss04/08_Long"},
                {"textBoxShort_05", "Truss05/07_Short"},
                {"textBoxLong_05", "Truss05/07_Long"},
                {"textBoxShort_06", "Truss06_Short"},
                {"textBoxLong_06", "Truss06_Long"},
                {"textBoxShort_07", "Truss05/07_Short"},
                {"textBoxLong_07", "Truss05/07_Long"},
                {"textBoxShort_08", "Truss04/08_Short"},
                {"textBoxLong_08", "Truss04/08_Long"},
                {"textBoxShort_09", "Truss03/09_Short"},
                {"textBoxLong_09", "Truss03/09_Long"},
                {"textBoxShort_10", "Truss02/10_Short"},
                {"textBoxLong_10", "Truss02/10_Long"},
                {"textBoxShort_11", "Truss01/11_Short"},
                {"textBoxLong_11", "Truss01/11_Long"},
                {"textBoxShort_B", "TrussA/B_Short"},
                {"textBoxLong_B", "TrussA/B_Long"}
            };
        }

        private static void InitializeCheckBoxMappingsMirror()
        {
            CheckBoxMappingsMirror = new Dictionary<string, string>
            {
                // Trusses
                {"checkBoxOnOff_A", "TrussA/B"},
                {"checkBoxOnOff_01", "Truss01/11"},
                {"checkBoxOnOff_02", "Truss02/10"},
                {"checkBoxOnOff_03", "Truss03/09"},
                {"checkBoxOnOff_04", "Truss04/08"},
                {"checkBoxOnOff_05", "Truss05/07"},
                {"checkBoxOnOff_06", "Truss06"},
                {"checkBoxOnOff_07", "Truss05/07"},
                {"checkBoxOnOff_08", "Truss04/08"},
                {"checkBoxOnOff_09", "Truss03/09"},
                {"checkBoxOnOff_10", "Truss02/10"},
                {"checkBoxOnOff_11", "Truss01/11"},
                {"checkBoxOnOff_B", "TrussA/B"},

                // Truss 2" Offsets
                {"checkBox_2Out_A", "TrussA/B_Offset_Outside"},
                {"checkBox_2In_A", "TrussA/B_Offset_Inside"},
                {"checkBox_2Out_01", "Truss01/11_Offset_Outside"},
                {"checkBox_2In_01", "Truss01/11_Offset_Inside"},
                {"checkBox_2Out_02", "Truss02/10_Offset_Outside"},
                {"checkBox_2In_02", "Truss02/10_Offset_Inside"},
                {"checkBox_2Out_03", "Truss03/09_Offset_Outside"},
                {"checkBox_2In_03", "Truss03/09_Offset_Inside"},
                {"checkBox_2Out_04", "Truss04/08_Offset_Outside"},
                {"checkBox_2In_04", "Truss04/08_Offset_Inside"},
                {"checkBox_2Out_05", "Truss05/07_Offset_Outside"},
                {"checkBox_2In_05", "Truss05/07_Offset_Inside"},
                {"checkBox_2Out_06", "Truss06_Offset_Left"},
                {"checkBox_2In_06", "Truss06_Offset_Right"},
                {"checkBox_2Out_07", "Truss05/07_Offset_Outside"},
                {"checkBox_2In_07", "Truss05/07_Offset_Inside"},
                {"checkBox_2Out_08", "Truss04/08_Offset_Outside"},
                {"checkBox_2In_08", "Truss04/08_Offset_Inside"},
                {"checkBox_2Out_09", "Truss03/09_Offset_Outside"},
                {"checkBox_2In_09", "Truss03/09_Offset_Inside"},
                {"checkBox_2Out_10", "Truss02/10_Offset_Outside"},
                {"checkBox_2In_10", "Truss02/10_Offset_Inside"},
                {"checkBox_2Out_11", "Truss01/11_Offset_Outside"},
                {"checkBox_2In_11", "Truss01/11_Offset_Inside"},
                {"checkBox_2Out_B", "TrussA/B_Offset_Outside"},
                {"checkBox_2In_B", "TrussA/B_Offset_Inside"},

                //Strands
                {"checkBoxTopOn", "Top Strands"},
                {"checkBoxBotOn", "Bottom Strands"},
                {"checkBoxTop_A", "A_Top"},
                {"checkBoxBot_A", "A_Bottom"},
                {"checkBoxTop_B", "B_Top"},
                {"checkBoxBot_B", "B_Bottom"},
                {"checkBoxTop_01", "01_Top"},
                {"checkBoxBot_01", "01_Bottom"},
                {"checkBoxTop_02", "02_Top"},
                {"checkBoxBot_02", "02_Bottom"},
                {"checkBoxTop_03", "03_Top"},
                {"checkBoxBot_03", "03_Bottom"},
                {"checkBoxTop_04", "04_Top"},
                {"checkBoxBot_04", "04_Bottom"},
                {"checkBoxTop_05", "05_Top"},
                {"checkBoxBot_05", "05_Bottom"},
                {"checkBoxTop_06", "06_Top"},
                {"checkBoxBot_06", "06_Bottom"},
                {"checkBoxTop_07", "07_Top"},
                {"checkBoxBot_07", "07_Bottom"},
                {"checkBoxTop_08", "08_Top"},
                {"checkBoxBot_08", "08_Bottom"},
                {"checkBoxTop_09", "09_Top"},
                {"checkBoxBot_09", "09_Bottom"},
                {"checkBoxTop_10", "10_Top"},
                {"checkBoxBot_10", "10_Bottom"},
                {"checkBoxTop_11", "11_Top"},
                {"checkBoxBot_11", "11_Bottom"},
                {"checkBoxTop_12", "12_Top"},
                {"checkBoxBot_12", "12_Bottom"},
                {"checkBoxTop_13", "13_Top"},
                {"checkBoxBot_13", "13_Bottom"},
                {"checkBoxTop_14", "14_Top"},
                {"checkBoxBot_14", "14_Bottom"},
                {"checkBoxTop_15", "15_Top"},
                {"checkBoxBot_15", "15_Bottom"},
                {"checkBoxTop_16", "16_Top"},
                {"checkBoxBot_16", "16_Bottom"},
                {"checkBoxTop_17", "17_Top"},
                {"checkBoxBot_17", "17_Bottom"},
                {"checkBoxTop_18", "18_Top"},
                {"checkBoxBot_18", "18_Bottom"},
                {"checkBoxTop_19", "19_Top"},
                {"checkBoxBot_19", "19_Bottom"},
                {"checkBoxTop_20", "20_Top"},
                {"checkBoxBot_20", "20_Bottom"},
            };
        }

        private static void InitializeTextBoxDimensionMappingsAdjust()
        {
            TextBoxDimensionMappingsAdjust = new Dictionary<string, string>
            {
                {"textBoxDD_Depth", "DD_Depth"},
                {"textBoxDD_Length", "DD_Length"},
                {"textBoxDD_Width", "DD_Width"},
                {"textBoxDD_BotJoint", "Bot_Joint_Size"},
                {"textBoxDD_LedgeJoint", "Ledge_Joint"}
            };
        }

        private static void InitializeTextBoxMappingsAdjust()
        {
            TextBoxMappingsAdjust = new Dictionary<string, string>
            {
                {"textBoxShort_A", "ME_TrussA_Short"},
                {"textBoxLong_A", "ME_TrussA_Long"},
                {"textBoxShort_01", "ME_Truss01_Short"},
                {"textBoxLong_01", "ME_Truss01_Long"},
                {"textBoxShort_02", "ME_Truss02_Short"},
                {"textBoxLong_02", "ME_Truss02_Long"},
                {"textBoxShort_03", "ME_Truss03_Short"},
                {"textBoxLong_03", "ME_Truss03_Long"},
                {"textBoxShort_04", "ME_Truss04_Short"},
                {"textBoxLong_04", "ME_Truss04_Long"},
                {"textBoxShort_05", "ME_Truss05_Short"},
                {"textBoxLong_05", "ME_Truss05_Long"},
                {"textBoxShort_06", "ME_Truss06_Short"},
                {"textBoxLong_06", "ME_Truss06_Long"},
                {"textBoxShort_07", "ME_Truss07_Short"},
                {"textBoxLong_07", "ME_Truss07_Long"},
                {"textBoxShort_08", "ME_Truss08_Short"},
                {"textBoxLong_08", "ME_Truss08_Long"},
                {"textBoxShort_09", "ME_Truss09_Short"},
                {"textBoxLong_09", "ME_Truss09_Long"},
                {"textBoxShort_10", "ME_Truss10_Short"},
                {"textBoxLong_10", "ME_Truss10_Long"},
                {"textBoxShort_11", "ME_Truss11_Short"},
                {"textBoxLong_11", "ME_Truss11_Long"},
                {"textBoxShort_B", "ME_TrussB_Short"},
                {"textBoxLong_B", "ME_TrussB_Long"}
            };
        }

        private static void InitializeCheckBoxMappingsAdjust()
        {
            CheckBoxMappingsAdjust = new Dictionary<string, string>
            {
                // Trusses
                {"checkBoxTrussOffset", "6.5\" Truss_Offset"},
                {"checkBoxOnOff_A", "ME_TrussA"},
                {"checkBoxOnOff_01", "ME_Truss01"},
                {"checkBoxOnOff_02", "ME_Truss02"},
                {"checkBoxOnOff_03", "ME_Truss03"},
                {"checkBoxOnOff_04", "ME_Truss04"},
                {"checkBoxOnOff_05", "ME_Truss05"},
                {"checkBoxOnOff_06", "ME_Truss06"},
                {"checkBoxOnOff_07", "ME_Truss07"},
                {"checkBoxOnOff_08", "ME_Truss08"},
                {"checkBoxOnOff_09", "ME_Truss09"},
                {"checkBoxOnOff_10", "ME_Truss10"},
                {"checkBoxOnOff_11", "ME_Truss11"},
                {"checkBoxOnOff_B", "ME_TrussB"},

                // Truss 2" Offsets
                {"checkBox_2Out_A", "TrussA_Offset_Outside"},
                {"checkBox_2In_A", "TrussA_Offset_Inside"},
                {"checkBox_2Out_01", "Truss01_Offset_Outside"},
                {"checkBox_2In_01", "Truss01_Offset_Inside"},
                {"checkBox_2Out_02", "Truss02_Offset_Outside"},
                {"checkBox_2In_02", "Truss02_Offset_Inside"},
                {"checkBox_2Out_03", "Truss03_Offset_Outside"},
                {"checkBox_2In_03", "Truss03_Offset_Inside"},
                {"checkBox_2Out_04", "Truss04_Offset_Outside"},
                {"checkBox_2In_04", "Truss04_Offset_Inside"},
                {"checkBox_2Out_05", "Truss05_Offset_Outside"},
                {"checkBox_2In_05", "Truss05_Offset_Inside"},
                {"checkBox_2Out_06", "Truss06_Offset_Left"},
                {"checkBox_2In_06", "Truss06_Offset_Right"},
                {"checkBox_2Out_07", "Truss07_Offset_Outside"},
                {"checkBox_2In_07", "Truss07_Offset_Inside"},
                {"checkBox_2Out_08", "Truss08_Offset_Outside"},
                {"checkBox_2In_08", "Truss08_Offset_Inside"},
                {"checkBox_2Out_09", "Truss09_Offset_Outside"},
                {"checkBox_2In_09", "Truss09_Offset_Inside"},
                {"checkBox_2Out_10", "Truss10_Offset_Outside"},
                {"checkBox_2In_10", "Truss10_Offset_Inside"},
                {"checkBox_2Out_11", "Truss11_Offset_Outside"},
                {"checkBox_2In_11", "Truss11_Offset_Inside"},
                {"checkBox_2Out_B", "TrussB_Offset_Outside"},
                {"checkBox_2In_B", "TrussB_Offset_Inside"},

                //Strands
                {"checkBoxTopOn", "Top Strands"},
                {"checkBoxBotOn", "Bottom Strands"},
                {"checkBoxTop_A", "A_Top"},
                {"checkBoxBot_A", "A_Bottom"},
                {"checkBoxTop_B", "B_Top"},
                {"checkBoxBot_B", "B_Bottom"},
                {"checkBoxTop_01", "01_Top"},
                {"checkBoxBot_01", "01_Bottom"},
                {"checkBoxTop_02", "02_Top"},
                {"checkBoxBot_02", "02_Bottom"},
                {"checkBoxTop_03", "03_Top"},
                {"checkBoxBot_03", "03_Bottom"},
                {"checkBoxTop_04", "04_Top"},
                {"checkBoxBot_04", "04_Bottom"},
                {"checkBoxTop_05", "05_Top"},
                {"checkBoxBot_05", "05_Bottom"},
                {"checkBoxTop_06", "06_Top"},
                {"checkBoxBot_06", "06_Bottom"},
                {"checkBoxTop_07", "07_Top"},
                {"checkBoxBot_07", "07_Bottom"},
                {"checkBoxTop_08", "08_Top"},
                {"checkBoxBot_08", "08_Bottom"},
                {"checkBoxTop_09", "09_Top"},
                {"checkBoxBot_09", "09_Bottom"},
                {"checkBoxTop_10", "10_Top"},
                {"checkBoxBot_10", "10_Bottom"},
                {"checkBoxTop_11", "11_Top"},
                {"checkBoxBot_11", "11_Bottom"},
                {"checkBoxTop_12", "12_Top"},
                {"checkBoxBot_12", "12_Bottom"},
                {"checkBoxTop_13", "13_Top"},
                {"checkBoxBot_13", "13_Bottom"},
                {"checkBoxTop_14", "14_Top"},
                {"checkBoxBot_14", "14_Bottom"},
                {"checkBoxTop_15", "15_Top"},
                {"checkBoxBot_15", "15_Bottom"},
                {"checkBoxTop_16", "16_Top"},
                {"checkBoxBot_16", "16_Bottom"},
                {"checkBoxTop_17", "17_Top"},
                {"checkBoxBot_17", "17_Bottom"},
                {"checkBoxTop_18", "18_Top"},
                {"checkBoxBot_18", "18_Bottom"},
                {"checkBoxTop_19", "19_Top"},
                {"checkBoxBot_19", "19_Bottom"},
                {"checkBoxTop_20", "20_Top"},
                {"checkBoxBot_20", "20_Bottom"},
            };
        }

        private static void InitializeTextBoxDimensionMappingsSkew()
        {
            TextBoxDimensionMappingsSkew = new Dictionary<string, string>
            {
                {"textBoxDD_Depth", "DD_Depth"},
                {"textBoxDD_Length", "DD_Length"},
                {"textBoxDD_Width", "DD_Width"},
                {"textBoxDD_BotJoint", "Joint_Size"},
                {"textBoxDD_LedgeJoint", "Ledge"},
                {"textBoxSkewOME", "skew angle OME"}
            };
        }

        private static void InitializeTextBoxMappingsSkew()
        {
            TextBoxMappingsSkew = new Dictionary<string, string>
            {
                {"textBoxShort_A", "ME_TrussA_Short"},
                {"textBoxLong_A", "ME_TrussA_Long"},
                {"textBoxShort_01", "ME_Truss01_Short"},
                {"textBoxLong_01", "ME_Truss01_Long"},
                {"textBoxShort_02", "ME_Truss02_Short"},
                {"textBoxLong_02", "ME_Truss02_Long"},
                {"textBoxShort_03", "ME_Truss03_Short"},
                {"textBoxLong_03", "ME_Truss03_Long"},
                {"textBoxShort_04", "ME_Truss04_Short"},
                {"textBoxLong_04", "ME_Truss04_Long"},
                {"textBoxShort_05", "ME_Truss05_Short"},
                {"textBoxLong_05", "ME_Truss05_Long"},
                {"textBoxShort_06", "ME_Truss06_Short"},
                {"textBoxLong_06", "ME_Truss06_Long"},
                {"textBoxShort_07", "ME_Truss07_Short"},
                {"textBoxLong_07", "ME_Truss07_Long"},
                {"textBoxShort_08", "ME_Truss08_Short"},
                {"textBoxLong_08", "ME_Truss08_Long"},
                {"textBoxShort_09", "ME_Truss09_Short"},
                {"textBoxLong_09", "ME_Truss09_Long"},
                {"textBoxShort_10", "ME_Truss10_Short"},
                {"textBoxLong_10", "ME_Truss10_Long"},
                {"textBoxShort_11", "ME_Truss11_Short"},
                {"textBoxLong_11", "ME_Truss11_Long"},
                {"textBoxShort_B", "ME_TrussB_Short"},
                {"textBoxLong_B", "ME_TrussB_Long"},
                //OME
                {"textBoxOME_Short_A", "OME_TrussA_Short"},
                {"textBoxOME_Long_A", "OME_TrussA_Long"},
                {"textBoxOME_Short_01", "OME_Truss01_Short"},
                {"textBoxOME_Long_01", "OME_Truss01_Long"},
                {"textBoxOME_Short_02", "OME_Truss02_Short"},
                {"textBoxOME_Long_02", "OME_Truss02_Long"},
                {"textBoxOME_Short_03", "OME_Truss03_Short"},
                {"textBoxOME_Long_03", "OME_Truss03_Long"},
                {"textBoxOME_Short_04", "OME_Truss04_Short"},
                {"textBoxOME_Long_04", "OME_Truss04_Long"},
                {"textBoxOME_Short_05", "OME_Truss05_Short"},
                {"textBoxOME_Long_05", "OME_Truss05_Long"},
                {"textBoxOME_Short_06", "OME_Truss06_Short"},
                {"textBoxOME_Long_06", "OME_Truss06_Long"},
                {"textBoxOME_Short_07", "OME_Truss07_Short"},
                {"textBoxOME_Long_07", "OME_Truss07_Long"},
                {"textBoxOME_Short_08", "OME_Truss08_Short"},
                {"textBoxOME_Long_08", "OME_Truss08_Long"},
                {"textBoxOME_Short_09", "OME_Truss09_Short"},
                {"textBoxOME_Long_09", "OME_Truss09_Long"},
                {"textBoxOME_Short_10", "OME_Truss10_Short"},
                {"textBoxOME_Long_10", "OME_Truss10_Long"},
                {"textBoxOME_Short_11", "OME_Truss11_Short"},
                {"textBoxOME_Long_11", "OME_Truss11_Long"},
                {"textBoxOME_Short_B", "OME_TrussB_Short"},
                {"textBoxOME_Long_B", "OME_TrussB_Long"}
            };
        }

        private static void InitializeCheckBoxMappingsSkew()
        {
            CheckBoxMappingsSkew = new Dictionary<string, string>
            {
                // Trusses
                {"checkBoxTrussOffset", "6.5\" Truss_Offset"},
                {"checkBoxOnOff_A", "ME_TrussA"},
                {"checkBoxOnOff_01", "ME_Truss01"},
                {"checkBoxOnOff_02", "ME_Truss02"},
                {"checkBoxOnOff_03", "ME_Truss03"},
                {"checkBoxOnOff_04", "ME_Truss04"},
                {"checkBoxOnOff_05", "ME_Truss05"},
                {"checkBoxOnOff_06", "ME_Truss06"},
                {"checkBoxOnOff_07", "ME_Truss07"},
                {"checkBoxOnOff_08", "ME_Truss08"},
                {"checkBoxOnOff_09", "ME_Truss09"},
                {"checkBoxOnOff_10", "ME_Truss10"},
                {"checkBoxOnOff_11", "ME_Truss11"},
                {"checkBoxOnOff_B", "ME_TrussB"},

                // Truss 2" Offsets
                {"checkBox_2Out_A", "TrussA_Offset_Outside"},
                {"checkBox_2In_A", "TrussA_Offset_Inside"},
                {"checkBox_2Out_01", "Truss01_Offset_Outside"},
                {"checkBox_2In_01", "Truss01_Offset_Inside"},
                {"checkBox_2Out_02", "Truss02_Offset_Outside"},
                {"checkBox_2In_02", "Truss02_Offset_Inside"},
                {"checkBox_2Out_03", "Truss03_Offset_Outside"},
                {"checkBox_2In_03", "Truss03_Offset_Inside"},
                {"checkBox_2Out_04", "Truss04_Offset_Outside"},
                {"checkBox_2In_04", "Truss04_Offset_Inside"},
                {"checkBox_2Out_05", "Truss05_Offset_Outside"},
                {"checkBox_2In_05", "Truss05_Offset_Inside"},
                {"checkBox_2Out_06", "Truss06_Offset_Left"},
                {"checkBox_2In_06", "Truss06_Offset_Right"},
                {"checkBox_2Out_07", "Truss07_Offset_Outside"},
                {"checkBox_2In_07", "Truss07_Offset_Inside"},
                {"checkBox_2Out_08", "Truss08_Offset_Outside"},
                {"checkBox_2In_08", "Truss08_Offset_Inside"},
                {"checkBox_2Out_09", "Truss09_Offset_Outside"},
                {"checkBox_2In_09", "Truss09_Offset_Inside"},
                {"checkBox_2Out_10", "Truss10_Offset_Outside"},
                {"checkBox_2In_10", "Truss10_Offset_Inside"},
                {"checkBox_2Out_11", "Truss11_Offset_Outside"},
                {"checkBox_2In_11", "Truss11_Offset_Inside"},
                {"checkBox_2Out_B", "TrussB_Offset_Outside"},
                {"checkBox_2In_B", "TrussB_Offset_Inside"},

                //Strands
                {"checkBoxTopOn", "Top Strands"},
                {"checkBoxBotOn", "Bottom Strands"},
                {"checkBoxTop_A", "A_Top"},
                {"checkBoxBot_A", "A_Bottom"},
                {"checkBoxTop_B", "B_Top"},
                {"checkBoxBot_B", "B_Bottom"},
                {"checkBoxTop_01", "01_Top"},
                {"checkBoxBot_01", "01_Bottom"},
                {"checkBoxTop_02", "02_Top"},
                {"checkBoxBot_02", "02_Bottom"},
                {"checkBoxTop_03", "03_Top"},
                {"checkBoxBot_03", "03_Bottom"},
                {"checkBoxTop_04", "04_Top"},
                {"checkBoxBot_04", "04_Bottom"},
                {"checkBoxTop_05", "05_Top"},
                {"checkBoxBot_05", "05_Bottom"},
                {"checkBoxTop_06", "06_Top"},
                {"checkBoxBot_06", "06_Bottom"},
                {"checkBoxTop_07", "07_Top"},
                {"checkBoxBot_07", "07_Bottom"},
                {"checkBoxTop_08", "08_Top"},
                {"checkBoxBot_08", "08_Bottom"},
                {"checkBoxTop_09", "09_Top"},
                {"checkBoxBot_09", "09_Bottom"},
                {"checkBoxTop_10", "10_Top"},
                {"checkBoxBot_10", "10_Bottom"},
                {"checkBoxTop_11", "11_Top"},
                {"checkBoxBot_11", "11_Bottom"},
                {"checkBoxTop_12", "12_Top"},
                {"checkBoxBot_12", "12_Bottom"},
                {"checkBoxTop_13", "13_Top"},
                {"checkBoxBot_13", "13_Bottom"},
                {"checkBoxTop_14", "14_Top"},
                {"checkBoxBot_14", "14_Bottom"},
                {"checkBoxTop_15", "15_Top"},
                {"checkBoxBot_15", "15_Bottom"},
                {"checkBoxTop_16", "16_Top"},
                {"checkBoxBot_16", "16_Bottom"},
                {"checkBoxTop_17", "17_Top"},
                {"checkBoxBot_17", "17_Bottom"},
                {"checkBoxTop_18", "18_Top"},
                {"checkBoxBot_18", "18_Bottom"},
                {"checkBoxTop_19", "19_Top"},
                {"checkBoxBot_19", "19_Bottom"},
                {"checkBoxTop_20", "20_Top"},
                {"checkBoxBot_20", "20_Bottom"},
            };
        }

        private static void InitializeTextBoxDimensionMappings2Skew()
        {
            TextBoxDimensionMappings2Skew = new Dictionary<string, string>
            {
                {"textBoxDD_Depth", "DD_Depth"},
                {"textBoxDD_Length", "DD_Length"},
                {"textBoxDD_Width", "DD_Width"},
                {"textBoxDD_BotJoint", "Joint_Size"},
                {"textBoxDD_LedgeJoint", "Ledge"},
                {"textBoxSkewOME", "skew angle OME"},
                {"textBoxSkewME", "skew angle ME"}
            };
        }

        private static void InitializeTextBoxMappings2Skew()
        {
            TextBoxMappings2Skew = new Dictionary<string, string>
            {
                {"textBoxShort_A", "ME_TrussA_Short"},
                {"textBoxLong_A", "ME_TrussA_Long"},
                {"textBoxShort_01", "ME_Truss01_Short"},
                {"textBoxLong_01", "ME_Truss01_Long"},
                {"textBoxShort_02", "ME_Truss02_Short"},
                {"textBoxLong_02", "ME_Truss02_Long"},
                {"textBoxShort_03", "ME_Truss03_Short"},
                {"textBoxLong_03", "ME_Truss03_Long"},
                {"textBoxShort_04", "ME_Truss04_Short"},
                {"textBoxLong_04", "ME_Truss04_Long"},
                {"textBoxShort_05", "ME_Truss05_Short"},
                {"textBoxLong_05", "ME_Truss05_Long"},
                {"textBoxShort_06", "ME_Truss06_Short"},
                {"textBoxLong_06", "ME_Truss06_Long"},
                {"textBoxShort_07", "ME_Truss07_Short"},
                {"textBoxLong_07", "ME_Truss07_Long"},
                {"textBoxShort_08", "ME_Truss08_Short"},
                {"textBoxLong_08", "ME_Truss08_Long"},
                {"textBoxShort_09", "ME_Truss09_Short"},
                {"textBoxLong_09", "ME_Truss09_Long"},
                {"textBoxShort_10", "ME_Truss10_Short"},
                {"textBoxLong_10", "ME_Truss10_Long"},
                {"textBoxShort_11", "ME_Truss11_Short"},
                {"textBoxLong_11", "ME_Truss11_Long"},
                {"textBoxShort_B", "ME_TrussB_Short"},
                {"textBoxLong_B", "ME_TrussB_Long"},
                //OME, only use these is asymmetrical, otherwise it will auto mirror
                {"textBoxOME_Short_A", "ASym_OME_TrussA_Short"},
                {"textBoxOME_Long_A", "ASym_OME_TrussA_Long"},
                {"textBoxOME_Short_01", "ASym_OME_Truss01_Short"},
                {"textBoxOME_Long_01", "ASym_OME_Truss01_Long"},
                {"textBoxOME_Short_02", "ASym_OME_Truss02_Short"},
                {"textBoxOME_Long_02", "ASym_OME_Truss02_Long"},
                {"textBoxOME_Short_03", "ASym_OME_Truss03_Short"},
                {"textBoxOME_Long_03", "ASym_OME_Truss03_Long"},
                {"textBoxOME_Short_04", "ASym_OME_Truss04_Short"},
                {"textBoxOME_Long_04", "ASym_OME_Truss04_Long"},
                {"textBoxOME_Short_05", "ASym_OME_Truss05_Short"},
                {"textBoxOME_Long_05", "ASym_OME_Truss05_Long"},
                {"textBoxOME_Short_06", "ASym_OME_Truss06_Short"},
                {"textBoxOME_Long_06", "ASym_OME_Truss06_Long"},
                {"textBoxOME_Short_07", "ASym_OME_Truss07_Short"},
                {"textBoxOME_Long_07", "ASym_OME_Truss07_Long"},
                {"textBoxOME_Short_08", "ASym_OME_Truss08_Short"},
                {"textBoxOME_Long_08", "ASym_OME_Truss08_Long"},
                {"textBoxOME_Short_09", "ASym_OME_Truss09_Short"},
                {"textBoxOME_Long_09", "ASym_OME_Truss09_Long"},
                {"textBoxOME_Short_10", "ASym_OME_Truss10_Short"},
                {"textBoxOME_Long_10", "ASym_OME_Truss10_Long"},
                {"textBoxOME_Short_11", "ASym_OME_Truss11_Short"},
                {"textBoxOME_Long_11", "ASym_OME_Truss11_Long"},
                {"textBoxOME_Short_B", "ASym_OME_TrussB_Short"},
                {"textBoxOME_Long_B", "ASym_OME_TrussB_Long"}
            };
        }

        private static void InitializeCheckBoxMappings2Skew()
        {
            CheckBoxMappings2Skew = new Dictionary<string, string>
            {
                // Trusses
                {"checkBoxTrussOffset", "6.5\" Truss_Offset"},
                {"checkBoxOnOff_A", "ME_TrussA"},
                {"checkBoxOnOff_01", "ME_Truss01"},
                {"checkBoxOnOff_02", "ME_Truss02"},
                {"checkBoxOnOff_03", "ME_Truss03"},
                {"checkBoxOnOff_04", "ME_Truss04"},
                {"checkBoxOnOff_05", "ME_Truss05"},
                {"checkBoxOnOff_06", "ME_Truss06"},
                {"checkBoxOnOff_07", "ME_Truss07"},
                {"checkBoxOnOff_08", "ME_Truss08"},
                {"checkBoxOnOff_09", "ME_Truss09"},
                {"checkBoxOnOff_10", "ME_Truss10"},
                {"checkBoxOnOff_11", "ME_Truss11"},
                {"checkBoxOnOff_B", "ME_TrussB"},

                // Truss 2" Offsets
                {"checkBox_2Out_A", "TrussA_Offset_Outside"},
                {"checkBox_2In_A", "TrussA_Offset_Inside"},
                {"checkBox_2Out_01", "Truss01_Offset_Outside"},
                {"checkBox_2In_01", "Truss01_Offset_Inside"},
                {"checkBox_2Out_02", "Truss02_Offset_Outside"},
                {"checkBox_2In_02", "Truss02_Offset_Inside"},
                {"checkBox_2Out_03", "Truss03_Offset_Outside"},
                {"checkBox_2In_03", "Truss03_Offset_Inside"},
                {"checkBox_2Out_04", "Truss04_Offset_Outside"},
                {"checkBox_2In_04", "Truss04_Offset_Inside"},
                {"checkBox_2Out_05", "Truss05_Offset_Outside"},
                {"checkBox_2In_05", "Truss05_Offset_Inside"},
                {"checkBox_2Out_06", "Truss06_Offset_Left"},
                {"checkBox_2In_06", "Truss06_Offset_Right"},
                {"checkBox_2Out_07", "Truss07_Offset_Outside"},
                {"checkBox_2In_07", "Truss07_Offset_Inside"},
                {"checkBox_2Out_08", "Truss08_Offset_Outside"},
                {"checkBox_2In_08", "Truss08_Offset_Inside"},
                {"checkBox_2Out_09", "Truss09_Offset_Outside"},
                {"checkBox_2In_09", "Truss09_Offset_Inside"},
                {"checkBox_2Out_10", "Truss10_Offset_Outside"},
                {"checkBox_2In_10", "Truss10_Offset_Inside"},
                {"checkBox_2Out_11", "Truss11_Offset_Outside"},
                {"checkBox_2In_11", "Truss11_Offset_Inside"},
                {"checkBox_2Out_B", "TrussB_Offset_Outside"},
                {"checkBox_2In_B", "TrussB_Offset_Inside"},

                //Strands
                {"checkBoxTopOn", "Top Strands"},
                {"checkBoxBotOn", "Bottom Strands"},
                {"checkBoxTop_A", "A_Top"},
                {"checkBoxBot_A", "A_Bottom"},
                {"checkBoxTop_B", "B_Top"},
                {"checkBoxBot_B", "B_Bottom"},
                {"checkBoxTop_01", "01_Top"},
                {"checkBoxBot_01", "01_Bottom"},
                {"checkBoxTop_02", "02_Top"},
                {"checkBoxBot_02", "02_Bottom"},
                {"checkBoxTop_03", "03_Top"},
                {"checkBoxBot_03", "03_Bottom"},
                {"checkBoxTop_04", "04_Top"},
                {"checkBoxBot_04", "04_Bottom"},
                {"checkBoxTop_05", "05_Top"},
                {"checkBoxBot_05", "05_Bottom"},
                {"checkBoxTop_06", "06_Top"},
                {"checkBoxBot_06", "06_Bottom"},
                {"checkBoxTop_07", "07_Top"},
                {"checkBoxBot_07", "07_Bottom"},
                {"checkBoxTop_08", "08_Top"},
                {"checkBoxBot_08", "08_Bottom"},
                {"checkBoxTop_09", "09_Top"},
                {"checkBoxBot_09", "09_Bottom"},
                {"checkBoxTop_10", "10_Top"},
                {"checkBoxBot_10", "10_Bottom"},
                {"checkBoxTop_11", "11_Top"},
                {"checkBoxBot_11", "11_Bottom"},
                {"checkBoxTop_12", "12_Top"},
                {"checkBoxBot_12", "12_Bottom"},
                {"checkBoxTop_13", "13_Top"},
                {"checkBoxBot_13", "13_Bottom"},
                {"checkBoxTop_14", "14_Top"},
                {"checkBoxBot_14", "14_Bottom"},
                {"checkBoxTop_15", "15_Top"},
                {"checkBoxBot_15", "15_Bottom"},
                {"checkBoxTop_16", "16_Top"},
                {"checkBoxBot_16", "16_Bottom"},
                {"checkBoxTop_17", "17_Top"},
                {"checkBoxBot_17", "17_Bottom"},
                {"checkBoxTop_18", "18_Top"},
                {"checkBoxBot_18", "18_Bottom"},
                {"checkBoxTop_19", "19_Top"},
                {"checkBoxBot_19", "19_Bottom"},
                {"checkBoxTop_20", "20_Top"},
                {"checkBoxBot_20", "20_Bottom"},

                // Misc.
                {"checkBoxAsymOME", "Mirror ME to OME"}
            };
        }

        private static void InitializeMirroringPairs()
        {
            mirroringPairsWithoutOffset = new Dictionary<string, string>
            {
                {"textBoxShort_A", "textBoxShort_B"},
                {"textBoxLong_A", "textBoxLong_B"},
                {"textBoxShort_01", "textBoxShort_11"},
                {"textBoxLong_01", "textBoxLong_11"},
                {"textBoxShort_02", "textBoxShort_10"},
                {"textBoxLong_02", "textBoxLong_10"},
                {"textBoxShort_03", "textBoxShort_09"},
                {"textBoxLong_03", "textBoxLong_09"},
                {"textBoxShort_04", "textBoxShort_08"},
                {"textBoxLong_04", "textBoxLong_08"},
                {"textBoxShort_05", "textBoxShort_07"},
                {"textBoxLong_05", "textBoxLong_07"},
                {"checkBoxOnOff_A", "checkBoxOnOff_B"},
                {"checkBoxOnOff_01", "checkBoxOnOff_11"},
                {"checkBoxOnOff_02", "checkBoxOnOff_10"},
                {"checkBoxOnOff_03", "checkBoxOnOff_09"},
                {"checkBoxOnOff_04", "checkBoxOnOff_08"},
                {"checkBoxOnOff_05", "checkBoxOnOff_07"},
                {"checkBox_2Out_A", "checkBox_2Out_B"},
                {"checkBox_2In_A", "checkBox_2In_B"},
                {"checkBox_2Out_01", "checkBox_2Out_11"},
                {"checkBox_2In_01", "checkBox_2In_11"},
                {"checkBox_2Out_02", "checkBox_2Out_10"},
                {"checkBox_2In_02", "checkBox_2In_10"},
                {"checkBox_2Out_03", "checkBox_2Out_09"},
                {"checkBox_2In_03", "checkBox_2In_09"},
                {"checkBox_2Out_04", "checkBox_2Out_08"},
                {"checkBox_2In_04", "checkBox_2In_08"},
                {"checkBox_2Out_05", "checkBox_2Out_07"},
                {"checkBox_2In_05", "checkBox_2In_07"}
            };

            mirroringPairsWithOffset = new Dictionary<string, string>
            {
                {"textBoxShort_A", "textBoxShort_11"},
                {"textBoxLong_A", "textBoxLong_11"},
                {"textBoxShort_01", "textBoxShort_10"},
                {"textBoxLong_01", "textBoxLong_10"},
                {"textBoxShort_02", "textBoxShort_09"},
                {"textBoxLong_02", "textBoxLong_09"},
                {"textBoxShort_03", "textBoxShort_08"},
                {"textBoxLong_03", "textBoxLong_08"},
                {"textBoxShort_04", "textBoxShort_07"},
                {"textBoxLong_04", "textBoxLong_07"},
                {"textBoxShort_05", "textBoxShort_06"},
                {"textBoxLong_05", "textBoxLong_06"},
                {"checkBoxOnOff_A", "checkBoxOnOff_11"},
                {"checkBoxOnOff_01", "checkBoxOnOff_10"},
                {"checkBoxOnOff_02", "checkBoxOnOff_09"},
                {"checkBoxOnOff_03", "checkBoxOnOff_08"},
                {"checkBoxOnOff_04", "checkBoxOnOff_07"},
                {"checkBoxOnOff_05", "checkBoxOnOff_06"},
                {"checkBox_2Out_A", "checkBox_2Out_11"},
                {"checkBox_2In_A", "checkBox_2In_11"},
                {"checkBox_2Out_01", "checkBox_2Out_10"},
                {"checkBox_2In_01", "checkBox_2In_10"},
                {"checkBox_2Out_02", "checkBox_2Out_09"},
                {"checkBox_2In_02", "checkBox_2In_09"},
                {"checkBox_2Out_03", "checkBox_2Out_08"},
                {"checkBox_2In_03", "checkBox_2In_08"},
                {"checkBox_2Out_04", "checkBox_2Out_07"},
                {"checkBox_2In_04", "checkBox_2In_07"},
                {"checkBox_2Out_05", "checkBox_2Out_06"},
                {"checkBox_2In_05", "checkBox_2In_06"}
            };
        }
    }
}
