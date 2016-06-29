namespace WhoIsTweeting
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.menuItemUser = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSignIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemConsumer = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAway = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOffline = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statOnline = new System.Windows.Forms.ToolStripStatusLabel();
            this.statAway = new System.Windows.Forms.ToolStripStatusLabel();
            this.statOffline = new System.Windows.Forms.ToolStripStatusLabel();
            this.listCtxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxItemNickname = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxItemID = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxItemMention = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxItemOpenProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.listUpdateWorker = new System.ComponentModel.BackgroundWorker();
            this.statUpdating = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.listBox = new WhoIsTweeting.UserListBox();
            this.menuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenu.SuspendLayout();
            this.mainLayout.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.listCtxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemUser});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.mainMenu.Size = new System.Drawing.Size(234, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // menuItemUser
            // 
            this.menuItemUser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.menuItemUser.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSignIn,
            this.menuItemConsumer,
            this.toolStripSeparator2,
            this.viewToolStripMenuItem,
            this.toolStripSeparator3,
            this.menuItemAbout,
            this.menuItemQuit});
            this.menuItemUser.Name = "menuItemUser";
            this.menuItemUser.Size = new System.Drawing.Size(71, 20);
            this.menuItemUser.Text = "Loading...";
            // 
            // menuItemSignIn
            // 
            this.menuItemSignIn.Name = "menuItemSignIn";
            this.menuItemSignIn.Size = new System.Drawing.Size(184, 22);
            this.menuItemSignIn.Text = "Sign &in with Twitter...";
            this.menuItemSignIn.Click += new System.EventHandler(this.OnSignInClick);
            // 
            // menuItemConsumer
            // 
            this.menuItemConsumer.Name = "menuItemConsumer";
            this.menuItemConsumer.Size = new System.Drawing.Size(184, 22);
            this.menuItemConsumer.Text = "Set &Consumer Key...";
            this.menuItemConsumer.Click += new System.EventHandler(this.OnConsumerClick);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAway,
            this.menuItemOffline});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // menuItemAway
            // 
            this.menuItemAway.Name = "menuItemAway";
            this.menuItemAway.Size = new System.Drawing.Size(141, 22);
            this.menuItemAway.Text = "A&way Users";
            this.menuItemAway.Click += new System.EventHandler(this.OnAwayClick);
            // 
            // menuItemOffline
            // 
            this.menuItemOffline.Name = "menuItemOffline";
            this.menuItemOffline.Size = new System.Drawing.Size(141, 22);
            this.menuItemOffline.Text = "&Offline Users";
            this.menuItemOffline.Click += new System.EventHandler(this.OnOfflineClick);
            // 
            // menuItemQuit
            // 
            this.menuItemQuit.Name = "menuItemQuit";
            this.menuItemQuit.Size = new System.Drawing.Size(184, 22);
            this.menuItemQuit.Text = "&Quit";
            this.menuItemQuit.Click += new System.EventHandler(this.OnQuitClick);
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.mainMenu, 0, 0);
            this.mainLayout.Controls.Add(this.statusStrip1, 0, 2);
            this.mainLayout.Controls.Add(this.listBox, 0, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.mainLayout.Size = new System.Drawing.Size(234, 567);
            this.mainLayout.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statOnline,
            this.statAway,
            this.statOffline,
            this.statUpdating});
            this.statusStrip1.Location = new System.Drawing.Point(0, 545);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(234, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statOnline
            // 
            this.statOnline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statOnline.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.statOnline.Name = "statOnline";
            this.statOnline.Size = new System.Drawing.Size(12, 17);
            this.statOnline.Text = "-";
            // 
            // statAway
            // 
            this.statAway.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statAway.ForeColor = System.Drawing.Color.LightGray;
            this.statAway.Name = "statAway";
            this.statAway.Size = new System.Drawing.Size(12, 17);
            this.statAway.Text = "-";
            // 
            // statOffline
            // 
            this.statOffline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statOffline.ForeColor = System.Drawing.Color.DimGray;
            this.statOffline.Name = "statOffline";
            this.statOffline.Size = new System.Drawing.Size(12, 17);
            this.statOffline.Text = "-";
            // 
            // listCtxMenu
            // 
            this.listCtxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxItemNickname,
            this.ctxItemID,
            this.toolStripSeparator1,
            this.ctxItemMention,
            this.ctxItemOpenProfile});
            this.listCtxMenu.Name = "listCtxMenu";
            this.listCtxMenu.ShowImageMargin = false;
            this.listCtxMenu.Size = new System.Drawing.Size(155, 98);
            // 
            // ctxItemNickname
            // 
            this.ctxItemNickname.Enabled = false;
            this.ctxItemNickname.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.ctxItemNickname.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ctxItemNickname.Name = "ctxItemNickname";
            this.ctxItemNickname.Size = new System.Drawing.Size(154, 22);
            this.ctxItemNickname.Text = "user";
            // 
            // ctxItemID
            // 
            this.ctxItemID.Enabled = false;
            this.ctxItemID.Name = "ctxItemID";
            this.ctxItemID.Size = new System.Drawing.Size(154, 22);
            this.ctxItemID.Text = "@user";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
            // 
            // ctxItemMention
            // 
            this.ctxItemMention.Name = "ctxItemMention";
            this.ctxItemMention.Size = new System.Drawing.Size(154, 22);
            this.ctxItemMention.Text = "Send a &Mention";
            this.ctxItemMention.Click += new System.EventHandler(this.OnCtxMentionClick);
            // 
            // ctxItemOpenProfile
            // 
            this.ctxItemOpenProfile.Name = "ctxItemOpenProfile";
            this.ctxItemOpenProfile.Size = new System.Drawing.Size(154, 22);
            this.ctxItemOpenProfile.Text = "Open Twitter &Profile";
            this.ctxItemOpenProfile.Click += new System.EventHandler(this.OnCtxOpenProfileClick);
            // 
            // listUpdateWorker
            // 
            this.listUpdateWorker.WorkerSupportsCancellation = true;
            this.listUpdateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.listUpdateWorker_DoWork);
            // 
            // statUpdating
            // 
            this.statUpdating.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.statUpdating.Name = "statUpdating";
            this.statUpdating.Size = new System.Drawing.Size(0, 17);
            this.statUpdating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(181, 6);
            // 
            // listBox
            // 
            this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBox.ContextMenuStrip = this.listCtxMenu;
            this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox.FormattingEnabled = true;
            this.listBox.ItemHeight = 20;
            this.listBox.Location = new System.Drawing.Point(0, 24);
            this.listBox.Margin = new System.Windows.Forms.Padding(0);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(234, 521);
            this.listBox.TabIndex = 3;
            this.listBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseUp);
            // 
            // menuItemAbout
            // 
            this.menuItemAbout.Name = "menuItemAbout";
            this.menuItemAbout.Size = new System.Drawing.Size(184, 22);
            this.menuItemAbout.Text = "&About...";
            this.menuItemAbout.Click += new System.EventHandler(this.OnAboutClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(234, 567);
            this.Controls.Add(this.mainLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.MainMenuStrip = this.mainMenu;
            this.Name = "MainForm";
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.listCtxMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemUser;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.ToolStripMenuItem menuItemQuit;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemAway;
        private System.Windows.Forms.ToolStripMenuItem menuItemOffline;
        private System.Windows.Forms.ToolStripMenuItem menuItemSignIn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statOnline;
        private System.Windows.Forms.ToolStripStatusLabel statAway;
        private System.Windows.Forms.ToolStripStatusLabel statOffline;
        private System.Windows.Forms.ToolStripMenuItem menuItemConsumer;
        private System.Windows.Forms.ContextMenuStrip listCtxMenu;
        private UserListBox listBox;
        private System.Windows.Forms.ToolStripMenuItem ctxItemNickname;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ctxItemOpenProfile;
        private System.Windows.Forms.ToolStripMenuItem ctxItemID;
        private System.Windows.Forms.ToolStripMenuItem ctxItemMention;
        private System.ComponentModel.BackgroundWorker listUpdateWorker;
        private System.Windows.Forms.ToolStripStatusLabel statUpdating;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuItemAbout;
    }
}

