using Mongoose.IDO;
using Mongoose.IDO.Protocol;
using System;

namespace ue_STY_EmployeeTerminationSyncFT
{
    [IDOExtensionClass("ue_STY_EmployeeTerminationSyncFT")]
    public class ue_STY_EmployeeTerminationSyncFT : ICERPLayer.ErpInteractionBase
    {
        [IDOMethod(MethodFlags.None, "Infobar")]
        public int STY_SetTerminatedEmployees(ref string Infobar)
        {
            int severity = 0;

            LoadCollectionResponseData SLEmpRequest;
            try
            {
               SLEmpRequest = this.ExecuteERPLoadCollectionRequest("SLEmployees", "EmpNum,TermDate", "TermDate <> NULL", "", 0, false);
            }catch(Exception e) {
                Infobar = e.Message;
                severity = 16;
                return severity;
            }

            if(SLEmpRequest.Items.Count > 0)
            {
                UpdateCollectionRequestData updateRequest = new UpdateCollectionRequestData("TAEmployees");
                for (int i = 0; i < SLEmpRequest.Items.Count; i++)
                {
                    string empNum = SLEmpRequest[i, "EmpNum"].GetValue("");
                    string termDate = SLEmpRequest[i, "TermDate"].GetValue("");

                    string filter = $"EmpNum = '{empNum.Trim()}'";

                    LoadCollectionResponseData empRequest = this.Context.Commands.LoadCollection("TAEmployees", "EmpNum,TermDate", filter, "", 1);

                    if (empRequest.Items.Count > 0)
                    {
                        if (!empRequest[0, "TermDate"].GetValue("").Equals(termDate))
                        {
                            IDOUpdateItem update = new IDOUpdateItem(UpdateAction.Update, empRequest.Items[0].ItemID);
                            update.Properties.Add("TermDate", termDate);
                            updateRequest.Items.Add(update);
                        }
                    }
                }
                this.Context.Commands.UpdateCollection(updateRequest);
            }
            else
            {
                Infobar = "No Employees to update.";
            }
            Infobar = "Process was successful.";
            return severity;
        }
    }
}
