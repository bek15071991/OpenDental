using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenDental.UI{ 
	public class ODGridCellList:List<OpenDental.UI.ODGridCell>{
		///<summary></summary>
		public void Add(string value) {
			this.Add(new ODGridCell(value));
		}

		///<summary>Creates a new ODGridCell with initial value of 'value' and starting index of 'idx'.  Meant to be used with combo box columns.</summary>
		public void Add(string value,int idx) {
			this.Add(new ODGridCell(value,idx));
		}
	}

	///<summary></summary>
	public class ODGridRow {
		public long RowNum;//gets incremented when added to an ODGridRowCollection.
		private ODGridCellList cells;
		private Color colorBackG;
		private bool bold;
		private Color colorText;
		private Color colorLborder;
		private object tag;
		private string note;
		///<summary>The current drop down state of the row.</summary>
		private ODGridDropDownState _dropDownState = ODGridDropDownState.None;
		///<summary>If this row is a dropdown child, then this should be set to the row that triggers it. Otherwise null.</summary>
		private ODGridRow _dropDownParent = null;
		///<summary>The height of this row. Doesn't include the note section of the row.</summary>
		private int _height;
		///<summary>The height of the note section of the row.</summary>
		private int _noteHeight;
		///<summary>The vertical location at which to start drawing this row in pixels.  This makes it much easier to paint rows.</summary>
		private int _rowLoc;

		///<summary>Creates a new ODGridRow.</summary>
		public ODGridRow(){
			cells=new ODGridCellList();
			colorBackG=Color.White;
			bold=false;
			colorText=Color.Black;
			colorLborder=Color.Empty;
			tag=null;
			note="";
			//height=0;
		}

		public ODGridRow(params string[] cellText) {
			cells=new ODGridCellList();
			cellText.ToList().ForEach(x => cells.Add(x));
			colorBackG=Color.White;
			bold=false;
			colorText=Color.Black;
			colorLborder=Color.Empty;
			tag=null;
			note="";
			//height=0;
		}

		public ODGridRow(params ODGridCell[] cellList) {
			cells=new ODGridCellList();
			cellList.ToList().ForEach(x => cells.Add(x));
			colorBackG=Color.White;
			bold=false;
			colorText=Color.Black;
			colorLborder=Color.Empty;
			tag=null;
			note="";
			//height=0;
		}

		///<summary></summary>
		public ODGridCellList	Cells{
			get{
				return cells;
			}
		}

	  ///<summary>Background color.</summary>
		public Color ColorBackG{
			get{
				return colorBackG;
			}
			set{
				colorBackG=value;
			}
		}

		///<summary></summary>
		public bool Bold{
			get{
				return bold;
			}
			set{
				bold=value;
			}
		}

		///<summary>This sets the text color for the whole row.  Each gridCell also has a colorText property that will override this if set.</summary>
		public Color ColorText{
			get{
				return colorText;
			}
			set{
				colorText=value;
			}
		}

		///<summary></summary>
		public Color ColorLborder{
			get{
				return colorLborder;
			}
			set{
				colorLborder=value;
			}
		}

		///<summary>Used to store any kind of object that is associated with the row.</summary>
		public object Tag{
			get{
				return tag;
			}
			set{
				tag=value;
			}
		}

		///<summary>This is a very special field.  Since most of the tables in OD require the ability to attach long notes to each row, this field makes it possible.  Any note set here will be drawn as a sort of subrow.  The note can span multiple columns, as defined in grid.NoteSpanStart and grid.NoteSpanStop.</summary>
		public string Note{
			get {
				return note;
			}
			set {
				note=value;
			}
		} 
		
		///<summary>The vertical height of only the note portion of the row in pixels.  Usually 0, unless you want notes showing.</summary>
		public int NoteHeight {
			get {
				return _noteHeight;
			}
			internal set {
				_noteHeight=value;
			}
		}

		///<summary>The vertical location at which to start drawing this row in pixels.  This makes it much easier to paint rows.</summary>
		public int RowLoc {
			get {
				return _rowLoc;
			}
			internal set {
				_rowLoc=value;
			}
		}

		///<summary>The vertical height of the row in pixels, not counting the note portion of the row.</summary>
		public int RowHeight {
			get {
				return _height;
			}
			internal set {
				_height=value;
			}
		}

		///<summary>The row height plus the note height.</summary>
		public int TotalHeight {
			get {
				return _height+_noteHeight;
			}
		}

		///<summary>If this is a dropdown row, set this to the index of the row that drops this row down.  If not, -1.</summary>
		public ODGridRow DropDownParent {
			get {
				return _dropDownParent;
			}
			set {
				_dropDownParent=value;
			}
		}

		///<summary>Does this row drop down other rows? -1: No; 0: Yes, currently not dropped; 1: Yes, currently dropped.
		///If unspecified, this will be automatically set if other rows list it as a drop down parent.</summary>
		public ODGridDropDownState DropDownState {
			get {
				return _dropDownState;
			}
			set {
				_dropDownState=value;
			}
		}

		/*
		///<Summary>If not set (0), then the row height is computed automatically.</Summary>
		public int Height {
			get {
				return height;
			}
			set {
				height=value;
			}
		}*/

		public ODGridRow Copy() {
			return (ODGridRow)this.MemberwiseClone();
		}

	}

	///<summary>Determines the state of a dropdown row.</summary>
	public enum ODGridDropDownState {
		///<summary>0 - not a drop down parent.</summary>  
		None,
		///<summary>1 - not dropped down.</summary>  
		Up,
		///<summary>2 - dropped down.</summary>
		Down,
	}











}






