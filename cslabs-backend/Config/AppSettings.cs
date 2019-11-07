﻿using System.Collections.Generic;

namespace CSLabsBackend.Config
{
    public class AppSettings
    {
        public string JWTSecret { get; set; }
        public string ModuleSpecialCode { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public RundeckSettings RunDeckApi { get; set; }
        public string[] CorsUrls { get; set; }
    }
}