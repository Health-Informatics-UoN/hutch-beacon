/*
 * Copyright [yyyy] [name of copyright owner]
   
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
   
       http://www.apache.org/licenses/LICENSE-2.0
   
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BeaconBridge.Constants.Submission;

namespace BeaconBridge.Models.Egress;

public class EgressSubmission
{
  public int Id { get; set; }
  public string? SubmissionId { get; set; }
  public EgressStatus Status { get; set; }
  public string? OutputBucket { get; set; }
  
  public DateTime? Completed { get; set; }
  public string? Reviewer { get; set; }
  public virtual List<EgressFile> Files { get; set; }

  public string? tesId { get; set; }

  public string? Name { get; set; }

  public string EgressStatusDisplay
  {
    get
    {
      var enumType = typeof(EgressStatus);
      var memberInfo = enumType.GetMember(Status.ToString());
      var displayAttribute = memberInfo.FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>();

      return displayAttribute?.Name ?? Status.ToString();
    }
  }
}
