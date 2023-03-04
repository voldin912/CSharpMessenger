using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Socket;

namespace WoWonder.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************

        public static GetSiteSettingsObject.ConfigObject SettingsSiteList;
        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
        public static ObservableCollection<UserDataObject> MyProfileList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<ChatObject> UserList = new ObservableCollection<ChatObject>();
        public static ObservableCollection<UserDataObject> ListCachedDataNearby = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<UserDataObject> FriendRequestsList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<UserDataObject> MyFollowingList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<Classes.Categories> ListCategoriesProducts = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.SharedFile> ListSharedFiles = new ObservableCollection<Classes.SharedFile>();
        public static ObservableCollection<Classes.SharedFile> LastSharedFiles = new ObservableCollection<Classes.SharedFile>();
        public static ObservableCollection<GroupChatRequest> GroupRequestsList = new ObservableCollection<GroupChatRequest>();
        public static ObservableCollection<Classes.OptionLastChat> MuteList = new ObservableCollection<Classes.OptionLastChat>();
        public static ObservableCollection<Classes.OptionLastChat> PinList = new ObservableCollection<Classes.OptionLastChat>();
        public static ObservableCollection<Classes.LastChatArchive> ArchiveList = new ObservableCollection<Classes.LastChatArchive>();
        public static ObservableCollection<DataTables.StickersTb> StickersList = new ObservableCollection<DataTables.StickersTb>();
        public static ObservableCollection<PrivateMessageObject> MessageUnreadList = new ObservableCollection<PrivateMessageObject>();
        public static ObservableCollection<string> NotifyShowList = new ObservableCollection<string>();

        public static List<Classes.StorageTypeSelectClass> StorageTypeWiFiSelect = new List<Classes.StorageTypeSelectClass>();
        public static List<Classes.StorageTypeSelectClass> StorageTypeMobileSelect = new List<Classes.StorageTypeSelectClass>();
         
        public static void ClearAllList()
        {
            try
            { 
                DataUserLoginList.Clear();
                MyProfileList.Clear();
                UserList.Clear();
                ListCachedDataNearby.Clear();
                FriendRequestsList.Clear();
                MyFollowingList.Clear();
                ListCategoriesProducts.Clear();
                ListSharedFiles.Clear();
                LastSharedFiles.Clear();
                GroupRequestsList.Clear();
                MuteList.Clear();
                PinList.Clear();
                ArchiveList.Clear();
                StickersList.Clear();
                MessageUnreadList.Clear();
                StorageTypeWiFiSelect.Clear();
                StorageTypeMobileSelect.Clear();
                NotifyShowList.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        }

        public static void Copy<T>(T from, T to)
        {
            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                try
                {
                    if (!p.CanRead || !p.CanWrite) continue;

                    object val = p.GetGetMethod().Invoke(from, null);
                    object defaultVal = p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;
                    if (null != defaultVal && !val.Equals(defaultVal))
                    {
                        p.GetSetMethod().Invoke(to, new[] { val });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static int Remove<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="condition">A function that evaluates to true for elements that should be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, Func<T, bool> condition)
        {
            // Find all elements satisfying the condition, i.e. that will be removed
            var toRemove = observableCollection
                .Where(condition)
                .ToList();

            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="toRemove">Find all elements satisfying the condition, i.e. that will be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, List<T> toRemove)
        {
            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }

    }
}