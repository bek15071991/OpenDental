namespace OpenDental {
	partial class FormRpUnfinalizedInsPay {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRpUnfinalizedInsPay));
			this.butClose = new OpenDental.UI.Button();
			this.labelType = new System.Windows.Forms.Label();
			this.comboBoxMultiType = new OpenDental.UI.ComboBoxMulti();
			this.labelClinic = new System.Windows.Forms.Label();
			this.comboBoxMultiClinics = new OpenDental.UI.ComboBoxMulti();
			this.gridMain = new OpenDental.UI.ODGrid();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.openEOBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteEOBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.goToAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openClaimToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.createCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.butExport = new OpenDental.UI.Button();
			this.butPrint = new OpenDental.UI.Button();
			this.textCarrier = new System.Windows.Forms.TextBox();
			this.labelCarrier = new System.Windows.Forms.Label();
			this.butRefresh = new OpenDental.UI.Button();
			this.groupBoxFilter = new System.Windows.Forms.GroupBox();
			this.contextMenuStrip1.SuspendLayout();
			this.groupBoxFilter.SuspendLayout();
			this.SuspendLayout();
			// 
			// butClose
			// 
			this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Autosize = true;
			this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClose.CornerRadius = 4F;
			this.butClose.Location = new System.Drawing.Point(894, 463);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 24);
			this.butClose.TabIndex = 2;
			this.butClose.Text = "&Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			// 
			// labelType
			// 
			this.labelType.Location = new System.Drawing.Point(4, 20);
			this.labelType.Name = "labelType";
			this.labelType.Size = new System.Drawing.Size(39, 16);
			this.labelType.TabIndex = 61;
			this.labelType.Text = "Type:";
			this.labelType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxMultiType
			// 
			this.comboBoxMultiType.ArraySelectedIndices = new int[0];
			this.comboBoxMultiType.BackColor = System.Drawing.SystemColors.Window;
			this.comboBoxMultiType.Items = ((System.Collections.ArrayList)(resources.GetObject("comboBoxMultiType.Items")));
			this.comboBoxMultiType.Location = new System.Drawing.Point(45, 18);
			this.comboBoxMultiType.Name = "comboBoxMultiType";
			this.comboBoxMultiType.SelectedIndices = ((System.Collections.ArrayList)(resources.GetObject("comboBoxMultiType.SelectedIndices")));
			this.comboBoxMultiType.Size = new System.Drawing.Size(160, 21);
			this.comboBoxMultiType.TabIndex = 62;
			this.comboBoxMultiType.SelectionChangeCommitted += new OpenDental.UI.ComboBoxMulti.SelectionChangeCommittedHandler(this.comboBoxMulti_SelectionChangeCommitted);
			// 
			// labelClinic
			// 
			this.labelClinic.Location = new System.Drawing.Point(207, 20);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(70, 16);
			this.labelClinic.TabIndex = 64;
			this.labelClinic.Text = "Clinic:";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelClinic.Visible = false;
			// 
			// comboBoxMultiClinics
			// 
			this.comboBoxMultiClinics.ArraySelectedIndices = new int[0];
			this.comboBoxMultiClinics.BackColor = System.Drawing.SystemColors.Window;
			this.comboBoxMultiClinics.Items = ((System.Collections.ArrayList)(resources.GetObject("comboBoxMultiClinics.Items")));
			this.comboBoxMultiClinics.Location = new System.Drawing.Point(280, 18);
			this.comboBoxMultiClinics.Name = "comboBoxMultiClinics";
			this.comboBoxMultiClinics.SelectedIndices = ((System.Collections.ArrayList)(resources.GetObject("comboBoxMultiClinics.SelectedIndices")));
			this.comboBoxMultiClinics.Size = new System.Drawing.Size(160, 21);
			this.comboBoxMultiClinics.TabIndex = 63;
			this.comboBoxMultiClinics.Visible = false;
			this.comboBoxMultiClinics.SelectionChangeCommitted += new OpenDental.UI.ComboBoxMulti.SelectionChangeCommittedHandler(this.comboBoxMulti_SelectionChangeCommitted);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.CellFont = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
			this.gridMain.ContextMenuStrip = this.contextMenuStrip1;
			this.gridMain.HasAddButton = false;
			this.gridMain.HasDropDowns = false;
			this.gridMain.HasMultilineHeaders = false;
			this.gridMain.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
			this.gridMain.HeaderHeight = 15;
			this.gridMain.HScrollVisible = false;
			this.gridMain.Location = new System.Drawing.Point(8, 64);
			this.gridMain.Name = "gridMain";
			this.gridMain.ScrollValue = 0;
			this.gridMain.Size = new System.Drawing.Size(961, 393);
			this.gridMain.TabIndex = 65;
			this.gridMain.Title = "Unfinalized Insurance Payment";
			this.gridMain.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.gridMain.TitleHeight = 18;
			this.gridMain.TranslationName = "TableUnfinalizedInsPay";
			this.gridMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gridMain_MouseUp);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEOBToolStripMenuItem,
            this.deleteEOBToolStripMenuItem,
            this.goToAccountToolStripMenuItem,
            this.openClaimToolStripMenuItem,
            this.createCheckToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(152, 114);
			// 
			// openEOBToolStripMenuItem
			// 
			this.openEOBToolStripMenuItem.Name = "openEOBToolStripMenuItem";
			this.openEOBToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.openEOBToolStripMenuItem.Text = "Open EOB";
			this.openEOBToolStripMenuItem.Click += new System.EventHandler(this.openEOBToolStripMenuItem_Click);
			// 
			// deleteEOBToolStripMenuItem
			// 
			this.deleteEOBToolStripMenuItem.Name = "deleteEOBToolStripMenuItem";
			this.deleteEOBToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.deleteEOBToolStripMenuItem.Text = "Delete EOB";
			this.deleteEOBToolStripMenuItem.Click += new System.EventHandler(this.deleteEOBToolStripMenuItem_Click);
			// 
			// goToAccountToolStripMenuItem
			// 
			this.goToAccountToolStripMenuItem.Name = "goToAccountToolStripMenuItem";
			this.goToAccountToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.goToAccountToolStripMenuItem.Text = "Go to Account";
			this.goToAccountToolStripMenuItem.Click += new System.EventHandler(this.goToAccountToolStripMenuItem_Click);
			// 
			// openClaimToolStripMenuItem
			// 
			this.openClaimToolStripMenuItem.Name = "openClaimToolStripMenuItem";
			this.openClaimToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.openClaimToolStripMenuItem.Text = "Open Claim";
			this.openClaimToolStripMenuItem.Click += new System.EventHandler(this.openClaimToolStripMenuItem_Click);
			// 
			// createCheckToolStripMenuItem
			// 
			this.createCheckToolStripMenuItem.Name = "createCheckToolStripMenuItem";
			this.createCheckToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.createCheckToolStripMenuItem.Text = "Create Check";
			this.createCheckToolStripMenuItem.Click += new System.EventHandler(this.createCheckToolStripMenuItem_Click);
			// 
			// butExport
			// 
			this.butExport.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butExport.Autosize = true;
			this.butExport.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butExport.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butExport.CornerRadius = 4F;
			this.butExport.Image = global::OpenDental.Properties.Resources.butExport;
			this.butExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butExport.Location = new System.Drawing.Point(93, 463);
			this.butExport.Name = "butExport";
			this.butExport.Size = new System.Drawing.Size(79, 24);
			this.butExport.TabIndex = 71;
			this.butExport.Text = "&Export";
			this.butExport.Click += new System.EventHandler(this.butExport_Click);
			// 
			// butPrint
			// 
			this.butPrint.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.Autosize = true;
			this.butPrint.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butPrint.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butPrint.CornerRadius = 4F;
			this.butPrint.Image = global::OpenDental.Properties.Resources.butPrintSmall;
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(8, 463);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(79, 24);
			this.butPrint.TabIndex = 70;
			this.butPrint.Text = "&Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// textCarrier
			// 
			this.textCarrier.Location = new System.Drawing.Point(519, 19);
			this.textCarrier.Name = "textCarrier";
			this.textCarrier.Size = new System.Drawing.Size(160, 20);
			this.textCarrier.TabIndex = 107;
			// 
			// labelCarrier
			// 
			this.labelCarrier.Location = new System.Drawing.Point(446, 21);
			this.labelCarrier.Name = "labelCarrier";
			this.labelCarrier.Size = new System.Drawing.Size(70, 16);
			this.labelCarrier.TabIndex = 108;
			this.labelCarrier.Text = "Carrier:";
			this.labelCarrier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butRefresh
			// 
			this.butRefresh.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butRefresh.Autosize = true;
			this.butRefresh.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butRefresh.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butRefresh.CornerRadius = 4F;
			this.butRefresh.Location = new System.Drawing.Point(874, 14);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(82, 24);
			this.butRefresh.TabIndex = 109;
			this.butRefresh.Text = "&Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// groupBoxFilter
			// 
			this.groupBoxFilter.Controls.Add(this.textCarrier);
			this.groupBoxFilter.Controls.Add(this.comboBoxMultiClinics);
			this.groupBoxFilter.Controls.Add(this.butRefresh);
			this.groupBoxFilter.Controls.Add(this.labelClinic);
			this.groupBoxFilter.Controls.Add(this.labelCarrier);
			this.groupBoxFilter.Controls.Add(this.comboBoxMultiType);
			this.groupBoxFilter.Controls.Add(this.labelType);
			this.groupBoxFilter.Location = new System.Drawing.Point(8, 2);
			this.groupBoxFilter.Name = "groupBoxFilter";
			this.groupBoxFilter.Size = new System.Drawing.Size(961, 56);
			this.groupBoxFilter.TabIndex = 110;
			this.groupBoxFilter.TabStop = false;
			this.groupBoxFilter.Text = "Filters";
			// 
			// FormRpUnfinalizedInsPay
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(981, 499);
			this.Controls.Add(this.groupBoxFilter);
			this.Controls.Add(this.butExport);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butClose);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(997, 538);
			this.Name = "FormRpUnfinalizedInsPay";
			this.Text = "Unfinalized Insurance Payment";
			this.Load += new System.EventHandler(this.FormRpInsPayPlansPastDue_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.groupBoxFilter.ResumeLayout(false);
			this.groupBoxFilter.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private OpenDental.UI.Button butClose;
		private System.Windows.Forms.Label labelType;
		private UI.ComboBoxMulti comboBoxMultiType;
		private System.Windows.Forms.Label labelClinic;
		private UI.ComboBoxMulti comboBoxMultiClinics;
		private UI.ODGrid gridMain;
		private UI.Button butExport;
		private UI.Button butPrint;
		public System.Windows.Forms.TextBox textCarrier;
		private System.Windows.Forms.Label labelCarrier;
		private UI.Button butRefresh;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem openEOBToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteEOBToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem goToAccountToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openClaimToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem createCheckToolStripMenuItem;
		private System.Windows.Forms.GroupBox groupBoxFilter;
	}
}