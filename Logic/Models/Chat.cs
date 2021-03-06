﻿using Nest;

namespace Logic.Models
{
    [ElasticsearchType]
    public class Chat
    {
        public Chat(string name)
        {
            Guid = System.Guid.NewGuid().ToString();
            Name = name;
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Guid { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Name { get; set; }
    }
}