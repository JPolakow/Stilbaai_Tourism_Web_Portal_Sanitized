using System.ComponentModel.DataAnnotations;

namespace Stilbaai_Tourism_Web_Portal.Models
{
   public class EventModel
   {
      public int EVENT_ID { get; set; }
      public string? EVENT_NAME { get; set; }
      public string? EVENT_NUM { get; set; }
      public string? EVENT_EMAIL { get; set; }
      public string? EVENT_WEBSITE { get; set; }
      public string? EVENT_ADDRESS { get; set; }
      public string? EVENT_PERSON { get; set; }
      public string? EVENT_DATE { get; set; }
      public string? EVENT_STARTTIME { get; set; }
      public string? EVENT_DURATION { get; set; }
      public string? EVENT_DESCRIPTION { get; set; }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------