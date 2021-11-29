using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapientServices.Data.Validation
{
    public class ApprovalValidation
    {

        /// <summary>
        /// validate a Deserialized Approval Message
        /// </summary>
        /// <param name="approval">the Approval Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateApproval(Approval approval)
        {
            string retval = string.Empty;
            if (!ValidApprovalStatus(approval.status))
            {
                retval = "Approval Invalid Status";
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid request command
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidApprovalStatus(string tag)
        {
            var retval = false;
            switch (tag)
            {
                case "Accepted":
                case "Approved":
                case "Rejected":
                case "Complete":
                    retval = true;
                    break;
            }

            return retval;
        }
    }
}
