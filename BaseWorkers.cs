using Server;
using Server.ContextMenus;
using Server.Engines.VendorSearching;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TajemniceCintry2
{
    [CorpseName("Cialo")]
    public class Robotnik : TCBaseCreature
    {
        private DateTime m_NextWeaponChange;
        private BaseHouse m_House;
        private LevelWorkerProfession m_exp;
        private WorkerProfession m_work;
        private int UsedToolWorker;
        private string m_WorkShopName;
        private int m_BankAccount;
        private int m_HoldGold;

        [Constructable]
        public Robotnik(Mobile owner, BaseHouse house, LevelWorkerProfession ExpLevelJobs, WorkerProfession Profession )
            : base(TCAIType.AI_Melee, TCFightMode.Aggressor, 10, 1, 0.15, 0.4)
        {
            Owner = owner;
            House = house;
            m_exp = ExpLevelJobs;
            m_work = Profession;

            WorkerSearch = true;

            InitStats(84, 72, 65);

            Female = false;
            Race = Race.Elf;
            Body = 400;
            Name = "Robotnik";
            Hue = 33770;
            HairItemID = 0x2Fc1;
            HairHue = 1145;
            FacialHairItemID = 0x203F;
            FacialHairHue = 1145;

            m_WorkShopName = "Brak Nazwy";

            SetSkill(SkillName.Swords, 80);
            SetSkill(SkillName.Anatomy, 70);
            SetSkill(SkillName.Tactics, 80);
            SetSkill(SkillName.Parry, 70);

            

            PackGold(2, 8);
            /*PackItem(new MieszczanskiMiecz());

            AddItem(new EleganckiKapeluszZPiorem(1895));
            AddItem(new NilfgaardzkieSpodnie(1050));
            AddItem(new ButyzKlamrami(1895));
            AddItem(new SrebrnaBransoleta());

            switch (Utility.Random(2))
            {
                case 0:
                    AddItem(new NilfgaardzkaKurtka(1111));
                    break;
                default:
                    AddItem(new Narzuta(2335));
                    AddItem(new KrotkaPeleryna(2335));
                    AddItem(new WiazanaKoszula(2123));
                    break;
            }*/
        }
        public void RenameShop(Mobile from)
        {
            from.SendLocalizedMessage(1062433); // Enter a new name for your shop (20 chars max):

            from.Prompt = new WorkerNamePrompt(this);
        }
        public void Rename(Mobile from)
        {
            from.SendLocalizedMessage(1062494); // Enter a new name for your vendor (20 characters max):

            from.Prompt = new WorkerNamePrompt(this);
        }

        private void ChangeWeapon()
        {
            if (Backpack == null)
                return;

            Item item = FindItemOnLayer(Layer.OneHanded);

            if (item == null)
                item = FindItemOnLayer(Layer.TwoHanded);

            System.Collections.Generic.List<BaseWeapon> weapons = new System.Collections.Generic.List<BaseWeapon>();

            foreach (Item i in Backpack.Items)
            {
                if (i is BaseWeapon && i != item)
                    weapons.Add((BaseWeapon)i);
            }

            if (weapons.Count > 0)
            {
                if (item != null)
                    Backpack.DropItem(item);

                AddItem(weapons[Utility.Random(weapons.Count)]);

                m_NextWeaponChange = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));
            }
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant != null && m_NextWeaponChange < DateTime.UtcNow)
                ChangeWeapon();
        }
        public BaseHouse House
        {
            get
            {
                return m_House;
            }
            set
            {
                if (m_House != null)
                    m_House.PlayerVendors.Remove(this);

                if (value != null)
                    value.PlayerVendors.Add(this);

                m_House = value;
            }
        }
        public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
        {            
            if (from.Alive)
            {
                list.Add(new JobsEntry(this, from, m_work, m_exp));

                if (from.Stabled.Count > 0)
                {
                    //list.Add(new ClaimAllEntry(this, from));
                }
            }

            base.AddCustomContextEntries(from, list);
        }
         public virtual bool IsOwner(Mobile m)
         {
            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (BaseHouse.NewVendorSystem && House != null)
            {
                return House.IsOwner(m) || m == Owner;
            }
            else
            {
                return m == Owner;
            }
         }
        public bool CanInteractWith(Mobile from, bool ownerOnly)
        {
            if (!from.CanSee(this) || !Utility.InUpdateRange(from, this) || !from.CheckAlive())
                return false;

            if (ownerOnly)
                return IsOwner(from);

            if (House != null && House.IsBanned(from) && !IsOwner(from))
            {
                from.SendLocalizedMessage(1062674); // You can't shop from this home as you have been banned from this establishment.
                return false;
            }

            return true;
        }

        public void AddUsedToolWorker(int i)
        {
            UsedToolWorker += i;            
        }
        public void SubtractUsedToolWorker(int i)
        {
            UsedToolWorker -= i;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string WorkShopName
        {
            get
            {
                return m_WorkShopName;
            }
            set
            {
                if (value == null)
                    m_WorkShopName = "";
                else
                    m_WorkShopName = value;

                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public string WorkerName
        {
            get
            {
                return Name;
            }
            set
            {
                if (value == null)
                    Name = "Robotnik";
                else
                    Name = value;

                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int BankAccount
        {
            get
            {
                return m_BankAccount;
            }
            set
            {
                m_BankAccount = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int HoldGold
        {
            get
            {
                return m_HoldGold;
            }
            set
            {
                m_HoldGold = value;
            }
        }
        public void CollectGold(Mobile to)
        {
            if (HoldGold > 0)
            {
                SayTo(to, "Posiadam {0} monet. Ile chcecie odebrac?", HoldGold.ToString());
                to.SendMessage("Wpisz ile pieniedzy chcesz odebrac (ESC = Anuluj):");

                to.Prompt = new CollectGoldPrompt(this);
            }
            else
            {
                //SayTo(to, 503215); // I am holding no gold for you.  // - to kasacji lub to zmiany
            }
        }
        public int GiveGold(Mobile to, int amount)
        {
            if (amount <= 0)
                return 0;

            if (amount > HoldGold)
            {
                SayTo(to, "W kasie jest {0} monet.", HoldGold.ToString());
                return 0;
            }

            int amountGiven = Banker.DepositUpTo(to, amount);
            HoldGold -= amountGiven;

            if (amountGiven > 0)
            {
                to.SendLocalizedMessage(1060397, amountGiven.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
            }

            if (amountGiven == 0)
            {
                SayTo(to, 1070755); // Your bank box cannot hold the gold you are requesting.  I will keep the gold until you can take it.
            }
            else if (amount > amountGiven)
            {
                SayTo(to, 1070756); // I can only give you part of the gold now, as your bank box is too full to hold the full amount.
            }
            else if (HoldGold > 0)
            {
                SayTo(to, 1042639); // Your gold has been transferred.
            }
            else
            {
                SayTo(to, 503234); // All the gold I have been carrying for you has been deposited into your bank account.
            }

            return amountGiven;
        }
        public override bool OnDragDrop(Mobile from, Item item)
        {
            if (!IsOwner(from))
            {
                SayTo(from, 503209); // I can only take item from the shop owner.
                return false;
            }

            if (item is IThisIsGold)
            {                
                if (item is Floren)
                {
                    item.Amount = item.Amount *100;
                }
                else if (item is Srebrnik)
                {
                    item.Amount = item.Amount * 10;
                }
               
                if (BaseHouse.NewVendorSystem)
                {
                    if (HoldGold < 1000000)
                    {
                        SayTo(from, 503210); // I'll take that to fund my services.

                        HoldGold += item.Amount;
                        item.Delete();

                        return true;
                    }
                    else
                    {
                        from.SendLocalizedMessage(1062493); // Your vendor has sufficient funds for operation and cannot accept this gold.

                        return false;
                    }
                }
                else
                {
                    if (BankAccount < 1000000)
                    {
                        SayTo(from, 503210); // I'll take that to fund my services.

                        BankAccount += item.Amount;
                        item.Delete();

                        return true;
                    }
                    else
                    {
                        from.SendLocalizedMessage(1062493); // Your vendor has sufficient funds for operation and cannot accept this gold.

                        return false;
                    }
                }
            }
            else
            {
                SayTo(from, 503211); // I can't carry any more.
                return false;
                /* bool newItem = (GetVendorItem(item) == null);

                 if (Backpack != null && Backpack.TryDropItem(from, item, false))
                 {
                     if (newItem)
                         OnItemGiven(from, item);

                     return true;
                 }
                 else
                 {
                     SayTo(from, 503211); // I can't carry any more.
                     return false;
                 }*/
            }
        }
        private class JobsEntry : ContextMenuEntry
        {
            private readonly Mobile m_From;
            private readonly Robotnik m_Trainer;
            private readonly WorkerProfession m_work;
            private readonly LevelWorkerProfession m_exp;
            public JobsEntry(Robotnik trainer, Mobile from, WorkerProfession work, LevelWorkerProfession exp)
                : base(6163, 4)
            {
                m_Trainer = trainer;
                m_From = from;
                m_work = work;
                m_exp = exp;
            }

            public override void OnClick()
            {
                ShowGump(m_From);
            }
            public virtual void ShowGump(Mobile from)
            {
                
                if (!from.Alive)
                {
                    from.SendLocalizedMessage(500949);
                    return;
                }
                if (this.CheckRange(from))
                {                    
                    from.SendGump(new RobotnikMenuGump(from, m_work, m_exp, m_Trainer));
                }
            }
            public virtual bool CheckRange(Mobile from)
            {
                if (from.AccessLevel >= AccessLevel.GameMaster)
                    return true;

                return false;
            }
        }
        public enum WorkerButtons
        {
            Medicine = 9001,
          
        }

        private class WorkShopNamePrompt : Prompt
        {
            private readonly Robotnik m_Worker;
            public WorkShopNamePrompt(Robotnik worker)
            {
                m_Worker = worker;
            }

            public override void OnResponse(Mobile from, string text)
            {
                if (!m_Worker.CanInteractWith(from, true))
                    return;

                string name = text.Trim();

                if (!NameVerification.Validate(name, 1, 20, true, true, true, 0, NameVerification.Empty))
                {
                    m_Worker.SayTo(from, "Nieprawidlowe imie.");
                    return;
                }

                m_Worker.WorkShopName = Utility.FixHtml(name);

                //from.SendGump(new RobotnikMenuGump(m_From, m_Work, m_exp, m_Worker));
            }
        }
        private class WorkerNamePrompt : Prompt
        {
            private readonly Robotnik m_Worker;
            public WorkerNamePrompt(Robotnik worker)
            {
                m_Worker = worker;
            }

            public override void OnResponse(Mobile from, string text)
            {
                if (!m_Worker.CanInteractWith(from, true))
                    return;

                string name = text.Trim();

                if (!NameVerification.Validate(name, 1, 20, true, true, true, 0, NameVerification.Empty))
                {
                    m_Worker.SayTo(from, "Nieprawidlowe imie.");
                    return;
                }

                m_Worker.Name = Utility.FixHtml(name);

                from.SendLocalizedMessage(1062496); // Your vendor has been renamed.

                //from.SendGump(new NewPlayerVendorOwnerGump(m_Vendor));
            }
        }
        private class CollectGoldPrompt : Prompt
        {
            private readonly Robotnik m_Worker;
            public CollectGoldPrompt(Robotnik worker)
            {
                m_Worker = worker;
            }

            public override void OnResponse(Mobile from, string text)
            {
                if (!m_Worker.CanInteractWith(from, true))
                    return;

                text = text.Trim();

                int amount;

                if (!int.TryParse(text, out amount))
                    amount = 0;

                GiveGold(from, amount);
            }

            public override void OnCancel(Mobile from)
            {
                if (!m_Worker.CanInteractWith(from, true))
                    return;

                GiveGold(from, 0);
            }

            private void GiveGold(Mobile to, int amount)
            {
                if (amount <= 0)
                {
                    m_Worker.SayTo(to, "W porzadku, pieniadze zostana w kasie.");
                }
                else
                {
                    m_Worker.GiveGold(to, amount);
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool WorkerSearch { get; set; }


        public Robotnik(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer)
        { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader)
        { base.Deserialize(reader); int version = reader.ReadInt(); }


    }

    public class RobotnikMenuGump : Gump
    {
        //private readonly EngineUniversity m_Uniwersytet;
        private readonly Mobile m_From;
        private readonly WorkerProfession m_Work;
        private readonly LevelWorkerProfession m_Exp;
        private readonly Robotnik m_Worker;
        public RobotnikMenuGump(Mobile from, WorkerProfession work, LevelWorkerProfession exp, Robotnik worker)
       : base(50, 200)
        {
            m_From = from;
            m_Work = work;
            m_Exp = exp;
            m_Worker = worker;
            string MyWork = work.ToString();
            string MyExp = exp.ToString();
            int goldHeld = worker.HoldGold;

            this.AddBackground(25, 10, 530, 180, 0x13BE);

            this.AddImageTiled(35, 20, 510, 160, 0xA40);
            this.AddAlphaRegion(35, 20, 510, 160);

            this.AddImage(10, 0, 0x28DC);
            this.AddImage(537, 175, 0x28DC);
            this.AddImage(10, 175, 0x28DC);
            this.AddImage(537, 0, 0x28DC);



            if (from is RentedVendor)
            {
                int days, hours;
                ((RentedVendor)from).ComputeRentalExpireDelay(out days, out hours);

                this.AddLabel(38, 132, 0x480, String.Format("Location rental will expire in {0} day{1} and {2} hour{3}.", days, days != 1 ? "s" : "", hours, hours != 1 ? "s" : ""));
            }
            /*AddHtml(200, 30, 275, 50, "<BASEFONT COLOR=#00008B>Informacje o Pracowniku:</BASEFONT>", false, false);

            this.AddHtml(200, 60, 275, 20, "<BASEFONT COLOR=#0000FF>Moj Zawod to:</BASEFONT>", false, false);
            this.AddHtml(200, 80, 275, 20, $"<BASEFONT COLOR=#0000FF>{MyWork}</BASEFONT>", false, false);
            this.AddHtml(200, 100, 275, 20, "<BASEFONT COLOR=#0000FF>Moj Wiedza o Pracy:</BASEFONT>", false, false);
            this.AddHtml(200, 120, 275, 20, $"<BASEFONT COLOR=#0000FF>{MyExp}</BASEFONT>", false, false);
            this.AddButton(80, 115, 9790, 9790, (int)WorkerButtons.Medicine, GumpButtonType.Page, 0);*/

            this.AddHtmlLocalized(40, 38, 260, 20, 1063900, 0x7FFF, false, false); // Proffesion Workers 
            this.AddLabel(300, 38, 0x480, work.ToString());

            this.AddHtmlLocalized(40, 62, 260, 20, 1063901, 0x7FFF, false, false); // Experience Workers: 
            this.AddLabel(300, 62, 0x480, exp.ToString());

            this.AddHtmlLocalized(40, 88, 260, 20, 1038324, 0x7FFF, false, false); // My charge per real world day is: 
                                                                                   //this.AddLabel(300, 58, 0x480, perRealWorldDay.ToString());

            this.AddHtmlLocalized(40, 112, 260, 20, 1038322, 0x7FFF, false, false); // Gold held in my account: 
            this.AddLabel(300, 112, 0x480, goldHeld.ToString());

            this.AddHtmlLocalized(40, 142, 260, 20, 1063906, 0x7FFF, false, false);

            this.AddButton(390, 24, 0x15E1, 0x15E5, 1, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(408, 21, 120, 20, 1063905, 0x7FFF, false, false); // Give Tool

            this.AddButton(390, 44, 0x15E1, 0x15E5, 8, GumpButtonType.Reply, 0); ;
            this.AddLabel(408, 41, 0x480, "Wyslij do Pracy");

            this.AddButton(390, 64, 0x15E1, 0x15E5, 3, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(408, 61, 120, 20, 1063903, 0x7FFF, false, false); // Rename WorkShop

            this.AddButton(390, 84, 0x15E1, 0x15E5, 4, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(408, 81, 120, 20, 1063904, 0x7FFF, false, false); // Rename Worker

            this.AddButton(390, 104, 0x15E1, 0x15E5, 5, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(408, 101, 120, 20, 3006123, 0x7FFF, false, false); // Open Paperdoll

            this.AddButton(390, 124, 0x15E1, 0x15E5, 6, GumpButtonType.Reply, 0);
            this.AddLabel(408, 121, 0x480, "Odbierz Materialy");

            this.AddButton(390, 144, 0x15E1, 0x15E5, 7, GumpButtonType.Reply, 0);
            this.AddLabel(408, 141, 0x480, "Zwolnij Pracownika");

            this.AddButton(390, 162, 0x15E1, 0x15E5, 0, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(408, 161, 120, 20, 1011012, 0x7FFF, false, false); // CANCEL
        }
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            /*if (info.ButtonID == 1 || info.ButtonID == 2) // See goods or Customize
                this.m_Vendor.CheckTeleport(from);

            if (!this.m_Vendor.CanInteractWith(from, true))
                return;*/

            switch (info.ButtonID)
            {
                case 1: // See goods
                    {
                        //this.m_Vendor.OpenBackpack(from);
                        from.Target = new TCWorkerGiveItemTarget(m_Work, m_Worker);                        
                        break;
                    }
                case 2: // Customize
                    {
                        //from.SendGump(new NewPlayerVendorCustomizeGump(this.m_Vendor));                        
                        break;
                    }
                case 3: // Rename Shop
                    {
                        this.m_Worker.RenameShop(from);                         
                        break;
                    }
                case 4: // Rename Vendor
                    {
                        this.m_Worker.Rename(from);                        
                        break;
                    }
                case 5: // Open Paperdoll
                    {
                        //this.m_Vendor.DisplayPaperdollTo(from);                        
                        break;
                    }
                case 6: // Collect Gold
                    {
                        this.m_Worker.CollectGold(from);                        
                        break;
                    }
                case 7: // Dismiss Vendor
                    {
                        //this.m_Vendor.Dismiss(from);                        
                        break;
                    }
                case 8: // Check last sold items
                    {
                        //this.m_Vendor.CheckSoldItems(from);                        
                        break;
                    }
            }
        }
    }


}
