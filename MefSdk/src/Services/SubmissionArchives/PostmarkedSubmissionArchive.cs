using System;

namespace MeF.Client.Services.InputComposition
{
    /// <summary>
    /// Contains properties and methods related to handling PostmarkedSubmissionArchives.
    /// </summary>
    public class PostmarkedSubmissionArchive
    {
        /// <summary>
        /// Gets or sets the post marked archive.
        /// </summary>
        /// <value>The post marked archive.</value>
        public SubmissionArchive PostMarkedArchive { get; set; }

        /// <summary>
        /// Gets or sets the post mark.
        /// </summary>
        /// <value>The post mark.</value>
        public DateTime PostMark { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostmarkedSubmissionArchive"/> class.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <param name="postMarkDate">The post mark date.</param>
        public PostmarkedSubmissionArchive(SubmissionArchive archive, DateTime postMarkDate)
        {
            PostMarkedArchive = archive;
            PostMark = postMarkDate;
        }

        /// <summary>
        /// Creates the post marked submission archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <param name="postMarkDate">The post mark date.</param>
        public void CreatePostMarkedSubmissionArchive(SubmissionArchive archive, DateTime postMarkDate)
        {
            PostMarkedArchive = archive;
            PostMark = postMarkDate;
        }
    }
}