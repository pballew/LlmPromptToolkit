using System;

namespace OllamaClient.Models
{
    /// <summary>
    /// Represents a stored prompt with metadata
    /// </summary>
    public class Prompt
    {
        /// <summary>
        /// Unique identifier for the prompt
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Human-readable name for the prompt
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The prompt content/text
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// When the prompt was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the prompt was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        public Prompt()
        {
            Id = Guid.NewGuid().ToString();
            Name = string.Empty;
            Content = string.Empty;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
