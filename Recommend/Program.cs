using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Recommend
{
    class Program
    {
        public static List<User> MyUsers;
        static void Main(string[] args)
        {
            Console.WriteLine("初始化数据");
            MyUsers = new List<User>();
            InitUser();
            Console.WriteLine("数据准备完毕...");
            Console.WriteLine("请输入推荐人数：");
            int needCount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("请输入用户名，URL地址名");
            string nameUrl = Console.ReadLine();
            nameUrl = nameUrl.TrimEnd('/');
            User user = MyUsers.Where(s => s.Url.Contains(nameUrl)).FirstOrDefault();
            if (user == null)
            {
                Console.WriteLine("未查到此用户！");
                Console.ReadLine();
                return;
            }
            int writeInId = user.Id;//准备推荐人
            //计算所有用户和输入用户的相似度，并排除掉相似度等于0的用户
            List<UserSorce> userSorces = new List<UserSorce>();
            foreach (var item in MyUsers)
            {
                if (item.Id == writeInId)
                    continue;
                UserSorce userSorce = new UserSorce();
                //使用Jaccard公式计算兴趣相似度
                double numerator = user.Interest.Intersect(item.Interest).Count();
                //排除交集等于0的
                if (numerator <= 0)
                    continue;
                double denominator = user.Interest.Union(item.Interest).Count();
                if (denominator == 0)
                    continue;
                double mark = numerator / denominator;

                userSorce.Id = item.Id;
                userSorce.Name = item.Name;
                userSorce.Url = item.Url;
                userSorce.Interest = item.Interest;
                userSorce.Sorce = mark;
                userSorces.Add(userSorce);
            }
            if (userSorces.Count <= 0)
                Console.WriteLine("无合适的推荐");

            userSorces = userSorces.OrderByDescending(s => s.Sorce).ToList();


            //选择最相似的n个用户
            Console.WriteLine("==========================");
            Console.WriteLine("与该用户关注最相似的" + needCount + "用户如下：");
            List<string> interests = new List<string>();
            for (int i = 0; i < needCount; i++)
            {
                UserSorce u = userSorces[i];
                Console.WriteLine("No." + (i + 1) + " 相似度[" + userSorces[i].Sorce.ToString("F4") + "]" + ":");
                Console.WriteLine(u.Name + "(" + u.Url + ")" + Environment.NewLine);

                if (u.Interest.Count() == 0)
                    continue;
                if (interests.Count() == 0)
                    interests = u.Interest;
                else
                    interests = interests.Union(u.Interest).ToList();
            }
            Console.WriteLine("==========================");

            //已经关注的用户
            List<string> IsFollowerUsers = user.Interest;
            //加上用户本身
            IsFollowerUsers.Add(user.Url);
            //所有相似度最高的用户关注的人
            List<string> exceptInterests = interests.Except(IsFollowerUsers).ToList();

            if (exceptInterests.Count <= 0)
                Console.WriteLine("无法推荐感兴趣的人！");

            List<InterestStar> stars = new List<InterestStar>();
            foreach (var interest in exceptInterests)
            {
                double value = 0;
                double depth = 1;//感兴趣的程度
                //推荐人
                string recommendUser = String.Empty;
                for (int i = 0; i < needCount; i++)
                {
                    User u = new User();
                    u = MyUsers.Where(s => s.Id == userSorces[i].Id).FirstOrDefault();
                    if (u.Interest.Contains(interest))
                    {
                        recommendUser += "[" + u.Name + "]";
                        double sorce = userSorces.Where(s => s.Id == u.Id).FirstOrDefault().Sorce * depth;
                        value += sorce;
                    }
                }

                InterestStar star = new InterestStar();
                var mUser = MyUsers.Where(s => s.Url.Contains(interest)).FirstOrDefault();
                if (mUser == null)
                    star.Name = interest;
                else
                    star.Name = mUser.Name + "(" + mUser.Url + ")";
                star.StarSorce = value;
                star.RecommendUser = recommendUser;
                stars.Add(star);
            }
            stars = stars.OrderByDescending(s => s.StarSorce).ToList();


            int starIndex = 1;
            Console.WriteLine(Environment.NewLine + "==========================");
            Console.WriteLine("推荐用户感兴趣的人：");
            foreach (var item in stars)
            {
                if (starIndex > needCount)
                    break;
                Console.WriteLine("No." + (starIndex++) + " 感兴趣程度[" + item.StarSorce.ToString("F4") + "]" + ":");
                Console.WriteLine("感兴趣的人：" + item.Name);
                Console.WriteLine("推荐的人有：" + item.RecommendUser + Environment.NewLine);
            }
            Console.WriteLine("==========================");
            Console.WriteLine("结束");
            Console.ReadKey();

        }

        static void InitUser()
        {
            XMLHelper xmlHelper = new XMLHelper("jobbole");
            xmlHelper.CreateXml();
            var members = xmlHelper.SelectAllMember();


            XMLHelper xmlFHelper = new XMLHelper("Follower");
            xmlFHelper.CreateXml();
            var followers = xmlFHelper.SelectAllFollower();

            foreach (var item in members)
            {
                User user = new User();
                user.Id = item.Id;
                user.Url = item.url;
                user.Name = item.name;
                var follower = followers.Where(s => s.NameUrl == item.url).FirstOrDefault();
                if (follower == null)
                    user.Interest = new List<string>();
                else
                    user.Interest = follower.FollowerUrl;
                MyUsers.Add(user);
            }
        }
    }

    /// <summary>
    /// 所有用户
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 个人主页
        /// </summary>
        public string Url { get; set; }

        public List<string> Interest { get; set; }

        //  public List<int> InterestDepth { get; set; }
    }

    /// <summary>
    /// 相似用户群
    /// </summary>
    public class UserSorce : User
    {
        public int Id { get; set; }
        /// <summary>
        /// 相似度分值
        /// </summary>
        public double Sorce { get; set; }
    }

    /// <summary>
    /// 推荐感兴趣的人
    /// </summary>
    public class InterestStar
    {
        /// <summary>
        /// 姓名+URL
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 感兴趣程度
        /// </summary>
        public double StarSorce { get; set; }

        /// <summary>
        /// 推荐人
        /// </summary>
        public string RecommendUser { get; set; }
    }
}
