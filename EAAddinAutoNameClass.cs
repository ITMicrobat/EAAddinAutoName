using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace EAAddinAutoName
{
	public class EAAddinAutoNameClass
	{
		

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