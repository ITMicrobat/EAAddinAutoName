using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace EAAddinAutoName
{
	public class EAAddinAutoNameClass
	{
		
		/*
		// define menu constants
		const string menuHeader = "-&MyAddin";
		const string menuHello = "&Say Shalom";
		const string menuGoodbye = "&Say Goodbye";
		private const string menuPackageList = "&Package List";

		// remember if we have to say hello or goodbye
		private bool shouldWeSayHello = true;
		*/

		///
		/// Called Before EA starts to check Add-In Exists
		/// Nothing is done here.
		/// This operation needs to exists for the addin to work
		///
		/// <param name="Repository" />the EA repository
		/// a string
		public String EA_Connect(EA.Repository Repository)
		{
			//No special processing required.
			return "a string";
		}

		/*
		///
		/// Called when user Clicks Add-Ins Menu item from within EA.
		/// Populates the Menu with our desired selections.
		/// Location can be "TreeView" "MainMenu" or "Diagram".
		///
		/// <param name="Repository" />the repository
		/// <param name="Location" />the location of the menu
		/// <param name="MenuName" />the name of the menu
		///
		public object EA_GetMenuItems(EA.Repository Repository, string Location, string MenuName)
		{
			switch (MenuName)
			{
				// defines the top level menu option
				case "":
					return menuHeader;
				// defines the submenu options
				case menuHeader:
					string[] subMenus = {menuHello, menuGoodbye, menuPackageList};
					return subMenus;
			}

			return "";
		}
		*/

		///
		/// returns true if a project is currently opened
		///
		/// <param name="Repository" />the repository
		/// true if a project is opened in EA
		bool IsProjectOpen(EA.Repository Repository)
		{
			try
			{
				EA.Collection c = Repository.Models;
				return true;
			}
			catch
			{
				return false;
			}
		}

		/*
		///
		/// Called once Menu has been opened to see what menu items should active.
		///
		/// <param name="Repository" />the repository
		/// <param name="Location" />the location of the menu
		/// <param name="MenuName" />the name of the menu
		/// <param name="ItemName" />the name of the menu item
		/// <param name="IsEnabled" />boolean indicating whethe the menu item is enabled
		/// <param name="IsChecked" />boolean indicating whether the menu is checked
		public void EA_GetMenuState(EA.Repository Repository, string Location, string MenuName, string ItemName,
			ref bool IsEnabled, ref bool IsChecked)
		{
			if (IsProjectOpen(Repository))
			{
				switch (ItemName)
				{
					// define the state of the hello menu option
					case menuHello:
						IsEnabled = shouldWeSayHello;
						break;
					// define the state of the goodbye menu option
					case menuGoodbye:
						IsEnabled = !shouldWeSayHello;
						break;
					case menuPackageList:
						IsEnabled = true;
						break;
					// there shouldn't be any other, but just in case disable it.
					default:
						IsEnabled = false;
						break;
				}
			}
			else
			{
				// If no open project, disable all menu options
				IsEnabled = false;
			}
		}
		*/

		/*
		///
		/// Called when user makes a selection in the menu.
		/// This is your main exit point to the rest of your Add-in
		///
		/// <param name="Repository" />the repository
		/// <param name="Location" />the location of the menu
		/// <param name="MenuName" />the name of the menu
		/// <param name="ItemName" />the name of the selected menu item
		public void EA_MenuClick(EA.Repository Repository, string Location, string MenuName, string ItemName)
		{
			switch (ItemName)
			{
				// user has clicked the menuHello menu option
				case menuHello:
					this.sayHello();
					break;
				// user has clicked the menuGoodbye menu option
				case menuGoodbye:
					this.sayGoodbye();
					break;
				case menuPackageList:
					this.packageList(Repository);
					break;
			}
		}
		*/

		/*
		///
		/// Say Hello to the world
		///
		private void sayHello()
		{
			MessageBox.Show("Hello World");
			this.shouldWeSayHello = false;
		}

		///
		/// Say Goodbye to the world
		///
		private void sayGoodbye()
		{
			MessageBox.Show("Goodbye World");
			this.shouldWeSayHello = true;
		}

		private void packageList(EA.Repository Repository)
		{
			string strMsg = "got following models: \n";
			foreach (EA.Package repositoryModel in Repository.Models)
			{
				strMsg += repositoryModel.Name + "\n";
			}
			MessageBox.Show(strMsg);
		}
		*/

		
		/*
		public bool EA_OnPreNewElement(EA.Repository repository, EA.EventProperties info)
		{
			MessageBox.Show("pre-new event");
			EA.Element element = this.getEventElement(repository, info);
			MessageBox.Show("type is: " + element.Type);
			MessageBox.Show("stereotype is: " + element.FQStereotype);

			
			return true;
		}
		*/

		public bool EA_OnPostNewElement(EA.Repository repository, EA.EventProperties info)
		{
			EA.Element element = this.getEventElement(repository, info);
			if ("Requirement" != element.Type)
			{
				return false;
			} 
				
			string strReplacement = this.getReplacement(element.FQStereotype);

			if ("" != strReplacement)
			{
				element.Name = element.Name.Replace("req-", strReplacement + "-");
				element.Update();
			}

			//true - there are changes, false - NO changes
			return "" != strReplacement;
		}

		/// <summary>
		/// get suitable replacement for 'req' substring in name
		/// </summary>
		/// <param name="strStereo"></param>
		/// <returns>returns replacement string for 'req' part of name, if required; empty string otherwise</returns>
		private string getReplacement(string strStereo)
		{
			string strReturn = "";
			// put required stereotype and corresponding prefix in here
			switch (strStereo)
			{
				case "EAREQ::BusinessRequirement":
					strReturn = "breq";
					break;
				case "EAREQ::FunctionalRequirement":
					strReturn = "freq";
					break;
				default:
					//we've got motivation
					//I want to add this prefix to any bmm-based requirement
					if (0 == strStereo.IndexOf("BMM::", 0))
					{
						strReturn = "mot";
					}
					break;
			}

			return strReturn;
		}

		private EA.Element getEventElement(EA.Repository rep, EA.EventProperties e)
		{
			var value = e.Get(0).Value;
			int intElementId = Convert.ToInt32(value);
			return rep.GetElementByID(intElementId);
		}

		//techical function, to debug-print an object
		private string printObject(object obj)
		{
			string strReturn = "";
			foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
			{
				string name=descriptor.Name;
				object value=descriptor.GetValue(obj);
				strReturn += string.Format("{0}={1}\n", name, value);
			}

			return strReturn;
		}

		///
		/// EA calls this operation when it exists. Can be used to do some cleanup work.
		///
		public void EA_Disconnect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}