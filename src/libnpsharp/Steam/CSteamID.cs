using System;

namespace NPSharp.Steam
{
    public class CSteamID
    {
        private readonly InteropHelp.BitVector64 steamid;

        public CSteamID()
            : this(0)
        {
        }

        internal CSteamID(UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType)
            : this()
        {
            Set(unAccountID, eUniverse, eAccountType);
        }

        internal CSteamID(UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType)
            : this()
        {
            InstancedSet(unAccountID, unInstance, eUniverse, eAccountType);
        }

        internal CSteamID(UInt64 id)
        {
            steamid = new InteropHelp.BitVector64(id);
        }

        internal CSteamID(SteamID_t sid)
            : this(
                sid.low32Bits, sid.high32Bits & 0xFFFFF, (EUniverse) (sid.high32Bits >> 24),
                (EAccountType) ((sid.high32Bits >> 20) & 0xF))
        {
        }

        public UInt32 AccountID
        {
            get { return (UInt32) steamid[0, 0xFFFFFFFF]; }
            set { steamid[0, 0xFFFFFFFF] = value; }
        }

        public UInt32 AccountInstance
        {
            get { return (UInt32) steamid[32, 0xFFFFF]; }
            set { steamid[32, 0xFFFFF] = value; }
        }

        public EAccountType AccountType
        {
            get { return (EAccountType) steamid[52, 0xF]; }
            set { steamid[52, 0xF] = (UInt64) value; }
        }

        public EUniverse AccountUniverse
        {
            get { return (EUniverse) steamid[56, 0xFF]; }
            set { steamid[56, 0xFF] = (UInt64) value; }
        }

        public static implicit operator UInt64(CSteamID sid)
        {
            return sid.steamid.Data;
        }

        public static implicit operator CSteamID(UInt64 id)
        {
            return new CSteamID(id);
        }

        public void Set(UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType)
        {
            AccountID = unAccountID;
            AccountUniverse = eUniverse;
            AccountType = eAccountType;

            if (eAccountType == EAccountType.Clan)
            {
                AccountInstance = 0;
            }
            else
            {
                AccountInstance = 1;
            }
        }

        public void InstancedSet(UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType)
        {
            AccountID = unAccountID;
            AccountUniverse = eUniverse;
            AccountType = eAccountType;
            AccountInstance = unInstance;
        }

        public void SetFromUint64(UInt64 ulSteamID)
        {
            steamid.Data = ulSteamID;
        }

        public UInt64 ConvertToUint64()
        {
            return steamid.Data;
        }

        public bool BBlankAnonAccount()
        {
            return AccountID == 0 && BAnonAccount() && AccountInstance == 0;
        }

        public bool BGameServerAccount()
        {
            return AccountType == EAccountType.GameServer ||
                   AccountType == EAccountType.AnonGameServer;
        }

        public bool BContentServerAccount()
        {
            return AccountType == EAccountType.ContentServer;
        }

        public bool BClanAccount()
        {
            return AccountType == EAccountType.Clan;
        }

        public bool BChatAccount()
        {
            return AccountType == EAccountType.Chat;
        }

        public bool IsLobby()
        {
            return (AccountType == EAccountType.Chat) && ((AccountInstance & (0x000FFFFF + 1) >> 2) != 0);
        }

        public bool BAnonAccount()
        {
            return AccountType == EAccountType.AnonUser ||
                   AccountType == EAccountType.AnonGameServer;
        }

        public bool BAnonUserAccount()
        {
            return AccountType == EAccountType.AnonUser;
        }

        public bool IsValid()
        {
            if (AccountType <= EAccountType.Invalid || AccountType >= EAccountType.Max)
                return false;

            if (AccountUniverse <= EUniverse.Invalid || AccountUniverse >= EUniverse.Max)
                return false;

            if (AccountType == EAccountType.Individual)
            {
                if (AccountID == 0 || AccountInstance != 1)
                    return false;
            }

            if (AccountType == EAccountType.Clan)
            {
                if (AccountID == 0 || AccountInstance != 0)
                    return false;
            }

            return true;
        }

        public string Render()
        {
            switch (AccountType)
            {
                case EAccountType.Invalid:
                case EAccountType.Individual:
                    return AccountUniverse <= EUniverse.Public
                        ? String.Format("STEAM_0:{0}:{1}", AccountID & 1, AccountID >> 1)
                        : String.Format("STEAM_{2}:{0}:{1}", AccountID & 1, AccountID >> 1, (int) AccountUniverse);
                default:
                    return Convert.ToString(this);
            }
        }

        public override string ToString()
        {
            return Render();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            var sid = obj as CSteamID;
            if (sid == null)
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        public bool Equals(CSteamID sid)
        {
            if (sid == null)
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        public static bool operator ==(CSteamID a, CSteamID b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if ((a == null) || (b == null))
                return false;

            return a.steamid.Data == b.steamid.Data;
        }

        public static bool operator !=(CSteamID a, CSteamID b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return steamid.Data.GetHashCode();
        }
    }
}