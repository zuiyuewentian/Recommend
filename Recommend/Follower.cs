using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Recommend
{
    public class Follower
    {
        public int Id { get; set; }

        public string NameUrl { get; set; }

        public List<string> FollowerUrl { get; set; }

        public int Count { get; set; }

    }
}
