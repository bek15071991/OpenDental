using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental.UI{ 

	///<summary></summary>
	public class ODGridCell{		
		private string text;
		private Color colorText;
		//private Color colorBackG; see auto-property below.
		private YN bold;
		private YN underline;
		///<summary>Defaults to -1 to mimic typical combo boxes.</summary>
		private int _selectedIndex=-1;
		
		///<summary>Creates a new ODGridCell.</summary>
		public ODGridCell(){
			text="";
			colorText=Color.Empty;
			bold=YN.Unknown;
			//colorBackG=Color.Empty;
		}

		///<summary>Creates a new ODGridCell.</summary>
		public ODGridCell(string myText){
			text=myText;
			colorText=Color.Empty;
			bold=YN.Unknown;
			underline=YN.Unknown;
		}

		///<summary>Creates a new ODGridCell with the initial value of 'myText' and then initial selected index of 'selectedIdx'.  
		///Meant to be used with combo box columns.</summary>
		public ODGridCell(string myText,int selectedIdx) {
			text=myText;
			_selectedIndex=selectedIdx;
			colorText=Color.Empty;
			bold=YN.Unknown;
		}

		public int SelectedIndex {
			get {
				return _selectedIndex;
			}
			set {
				_selectedIndex=value;
			}
		}

		///<summary></summary>
		public string Text{
			get{
				return text;
			}
			set{
				text=value;
			}
		}

		///<summary>Default is Color.Empty.  If any color is set, it will override the row color.</summary>
		public Color ColorText{
			get{
				return colorText;
			}
			set{
				colorText=value;
			}
		}

		///<summary>If YN.Unknown, then the row state is used for bold.  Otherwise, this overrides the row.</summary>
		public YN Bold{
			get{
				return bold;
			}
			set{
				bold=value;
			}
		}

		///<summary>If YN.Unknown, then the row state is used for underline.  Otherwise, this overrides the row.</summary>
		public YN Underline {
			get {
				return underline;
			}
			set {
				underline=value;
			}
		}

		///<summary>If set, colors only this cell.</summary>
		public Color CellColor { get; set; }
	        
	        

	}

	









}






