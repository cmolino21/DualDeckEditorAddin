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
        public static Dictionary<string, string> TextBoxMappings { get; private set; }
        public static Dictionary<string, string> CheckBoxMappings { get; private set; }

        static ControlMappings()
        {
            InitializeTextBoxMappings();
            InitializeCheckBoxMappings();
        }

        private static void InitializeTextBoxMappings()
        {
            TextBoxMappings = new Dictionary<string, string>
            {
                {"textBoxDD_Depth", "DD_Depth"},
                {"textBoxDD_Length", "DD_Length"},
                {"textBoxDD_Width", "DD_Width"},
                {"textBoxDD_BotJoint", "Bot_Joint_Size"},
                {"textBoxDD_LedgeJoint", "Ledge+Joint"},
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

        private static void InitializeCheckBoxMappings()
        {
            CheckBoxMappings = new Dictionary<string, string>
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
    }
}
