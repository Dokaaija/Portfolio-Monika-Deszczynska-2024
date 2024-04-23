using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TajemniceCintry2
{
    public class CowardAI : TCBaseAI
    {
        public CowardAI(TCBaseCreature m) : base(m) { }

        private int RangeFocusMob { get { return m_Mobile.RangePerception * 4; } }
        public override bool DoActionWander()
        {
            Mobile controller = m_Mobile.ControlMaster;

            if (m_Mobile.Combatant != null)
                Action = TCActionType.Flee;
            else if (controller != null && controller.Combatant != null && controller.GetDistanceToSqrt(m_Mobile) < 5)
                Action = TCActionType.Flee;
            else if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.FocusMob is TCBaseCreature && ((TCBaseCreature)m_Mobile.FocusMob).ControlMaster != null)
                {
                    m_Mobile.DebugSay("I have detected tamed {0}, not fleeing", m_Mobile.FocusMob.Name);
                }
                else
                {
                    m_Mobile.DebugSay("I have detected {0}, fleeing", m_Mobile.FocusMob.Name);
                    m_Mobile.Combatant = m_Mobile.FocusMob;
                    Action = TCActionType.Flee;
                }
            }

            else return base.DoActionWander();

            return true;
        }

        public override bool DoActionCombat()
        {
            Action = TCActionType.Wander;
            return true;
        }

        public override bool DoActionBackoff()
        {
            double hitPercent = (double)m_Mobile.Hits / m_Mobile.HitsMax;

            if (!m_Mobile.Summoned && !m_Mobile.Controlled && hitPercent < 0.1) Action = TCActionType.Flee;
            else
            {
                if (AcquireFocusMob(RangeFocusMob, TCFightMode.Closest, true, false, true))
                {
                    if (WalkMobileRange(m_Mobile.FocusMob, 1, false, m_Mobile.RangePerception, RangeFocusMob))
                        Action = TCActionType.Wander;
                }
                else Action = TCActionType.Wander;
            }

            return true;
        }

        public override bool DoActionFlee()
        {
            AcquireFocusMob(RangeFocusMob, m_Mobile.FightMode, true, false, true);

            if (m_Mobile.FocusMob == null) m_Mobile.FocusMob = m_Mobile.Combatant;
            return base.DoActionFlee();
        }
    }
}
