using System.ComponentModel.DataAnnotations.Schema;

namespace Instagraph.Models
{
    public class UserFollower
    {
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey(nameof(Follower))]
        public int FollowerId { get; set; }
        public virtual User Follower { get; set; }
    }
}
