using Microsoft.AspNetCore.Authorization;
using Stilbaai_Tourism_Web_Portal.DBHandelers;
using Stilbaai_Tourism_Web_Portal.Models;

namespace Stilbaai_Tourism_Web_Portal.Classes
{
   [Authorize]
   public class ToolBoxSingleton
   {
      //ensure only one instance is created, thus keeping the data
      private static readonly ToolBoxSingleton instance = new ToolBoxSingleton();
      public static ToolBoxSingleton Instance => instance;

      //api handeler instance
      public RestAPIHandeler aPIHandeler = new RestAPIHandeler();

      //---------------------------------------------------------------------------------------
      //eats
      private List<EatModel> eatsList = new List<EatModel>();
      public List<EatModel> EatsList { get => eatsList; set => eatsList = value; }

      //---------------------------------------------------------------------------------------
      //stay
      private List<StayModel> stayList = new List<StayModel>();
      public List<StayModel> StayList { get => stayList; set => stayList = value; }

      //---------------------------------------------------------------------------------------
      //stay category
      private List<StayCategoryModel> stayCategoryList = new List<StayCategoryModel>();
      public List<StayCategoryModel> StayCategoryList { get => stayCategoryList; set => stayCategoryList = value; }

      //---------------------------------------------------------------------------------------
      //activity
      private List<ActivityModel> activityList = new List<ActivityModel>();
      public List<ActivityModel> ActivityList { get => activityList; set => activityList = value; }

      //---------------------------------------------------------------------------------------
      //activity category
      private List<ActivityCategoryModel> activityCategoryList = new List<ActivityCategoryModel>();
      public List<ActivityCategoryModel> ActivityCategoryList { get => activityCategoryList; set => activityCategoryList = value; }

      //---------------------------------------------------------------------------------------
      //buisness
      private List<BusinessModel> businessList = new List<BusinessModel>();
      public List<BusinessModel> BusinessList { get => businessList; set => businessList = value; }

      //---------------------------------------------------------------------------------------
      //buisness category
      private List<BusinessCategoryModel> businessCategoryList = new List<BusinessCategoryModel>();
      public List<BusinessCategoryModel> BusinessCategoryList { get => businessCategoryList; set => businessCategoryList = value; }

      //---------------------------------------------------------------------------------------
      //contact
      private List<ContactModel> contactList = new List<ContactModel>();
      public List<ContactModel> ContactList { get => contactList; set => contactList = value; }

      //---------------------------------------------------------------------------------------
      //eel
      private List<EelModel> eelList = new List<EelModel>();
      public List<EelModel> EelList { get => eelList; set => eelList = value; }

      //---------------------------------------------------------------------------------------
      //event
      private List<EventModel> eventList = new List<EventModel>();
      public List<EventModel> EventList { get => eventList; set => eventList = value; }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------