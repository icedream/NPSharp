using System;
using System.Collections.Generic;
using BCryptClass = BCrypt.Net.BCrypt;

namespace NPSharp.CommandLine.Server.Database
{
    class Session
    {
        public Session()
        {
            Id = Guid.NewGuid().ToString("N");
        }
        public string Id { get; set; }
        public User User { get; set; }
        public DateTime ExpiryTime { get; set; }
        public TimeSpan ExpiresIn { get { return ExpiryTime - DateTime.Now; } }
        public bool IsValid { get { return ExpiryTime >= DateTime.Now; } }
    }

    class User
    {
        public uint Id { get; set; }
        public string UserName { get; set; }
        public string UserMail { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }
        public List<string> BanIDs { get; set; }
        public List<string> CheatDetectionIDs { get; set; }
        public List<uint> FriendIDs { get; set; } 

        public bool ComparePassword(string pw)
        {
            return BCryptClass.Verify(pw, PasswordHash);
        }
    }

    class UserFile
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public uint UserID { get; set; }
        public byte[] FileData { get; set; }
    }

    class PublisherFile
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
    }

    class Ban
    {
        public Ban()
        {
            Id = Guid.NewGuid().ToString("N");
        }
        public string Id { get; set; }
        public string Reason { get; set; }
        public DateTime ExpiryTime { get; set; }
        public TimeSpan ExpiresIn { get { return ExpiryTime - DateTime.Now; } }
        public bool IsValid { get { return ExpiryTime >= DateTime.Now; } }
    }

    class CheatDetection
    {
        public CheatDetection()
        {
            Id = Guid.NewGuid().ToString("N");
        }
        public string Id { get; set; }
        public uint CheatId { get; set; }
        public uint UserId { get; set; }
        public string Reason { get; set; }
        public DateTime ExpiryTime { get; set; }
        public TimeSpan ExpiresIn { get { return ExpiryTime - DateTime.Now; } }
        public bool IsValid { get { return ExpiryTime >= DateTime.Now; } }
    }
}
