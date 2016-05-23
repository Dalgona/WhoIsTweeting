namespace WhoIsTweeting
{
    partial class Form1
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
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.menuItemUser = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSignIn = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAway = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOffline = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statOnline = new System.Windows.Forms.ToolStripStatusLabel();
            this.statAway = new System.Windows.Forms.ToolStripStatusLabel();
            this.statOffline = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenu.SuspendLayout();
            this.mainLayout.SuspendLayout();
            this.statusStrip1.SuspendLayout();
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
            this.mainMenu.Size = new System.Drawing.Size(451, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // menuItemUser
            // 
            this.menuItemUser.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSignIn,
            this.viewToolStripMenuItem,
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
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.mainLayout.Size = new System.Drawing.Size(451, 567);
            this.mainLayout.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statOnline,
            this.statAway,
            this.statOffline});
            this.statusStrip1.Location = new System.Drawing.Point(0, 545);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(451, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statOnline
            // 
            this.statOnline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statOnline.Name = "statOnline";
            this.statOnline.Size = new System.Drawing.Size(12, 17);
            this.statOnline.Text = "-";
            // 
            // statAway
            // 
            this.statAway.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statAway.Name = "statAway";
            this.statAway.Size = new System.Drawing.Size(12, 17);
            this.statAway.Text = "-";
            // 
            // statOffline
            // 
            this.statOffline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statOffline.Name = "statOffline";
            this.statOffline.Size = new System.Drawing.Size(12, 17);
            this.statOffline.Text = "-";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(234, 567);
            this.Controls.Add(this.mainLayout);
            this.MainMenuStrip = this.mainMenu;
            this.Name = "Form1";
            this.Text = "Followings";
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
    }
}

