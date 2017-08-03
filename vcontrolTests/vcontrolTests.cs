using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ccVcontrol;
using System.Collections.Generic;
using ccInfo;

namespace vcontrolTests
{

    public class FakeProcessingContext : ProcessingContext
    {
        public List<ccPoint> mouseMoveTos = new List<ccPoint>();
        public List<ccPoint> mouseMoveAndClick = new List<ccPoint>();

        public FakeProcessingContext() : base(null, null, null)
        {
        }

        public override void MouseMouseTo(int x, int y)
        {
            mouseMoveTos.Add(new ccPoint(x, y));
        }
        public override void MoveMouseAndClick(int x, int y)
        {
            mouseMoveAndClick.Add(new ccPoint(x, y));
        }
    }

    public class TestSwtchAct : BaseProcessor
    {
        string failStep;
        public static List<StepInfo> switchSteps = new List<StepInfo>
            {
                new StepInfo { inputName= "chk_act_1stb.png", cmd = "-match settingsbutton.png 10", maxRetry = 2, name = "FindSettingsButton", xoff = 10, yoff = 10, delay = 10 },
                new StepInfo { inputName= "chk_act_2psi.png", cmd = "-match googlePlaySignIn.png 700", maxRetry = 3, name = "FindPlaySignin", xoff = 101, yoff = 63, delay = 50, otherStepCheck = new [] { "FindPlaySigninDisconnected" } },
                new StepInfo { inputName= "chk_act_3dsc.png", cmd = "-match googlePlayDisconnected.png 1700", maxRetry = 3, name = "FindPlaySigninDisconnected", xoff = 101, yoff = 63 , delay = 50},
                new StepInfo { inputName= "chk_act_4als.png", cmd = "-match accountlist.png 6000", maxRetry = 50, name = "SwitchAccount", xoff = 107, yoff = 89 , delay = 1},
        };
        public TestSwtchAct(ProcessingContext ctx, string failOn) : base(ctx)
        {
            failStep = failOn;
        }

        public override StepContext Process()
        {
            
            return DoSteps(switchSteps);
        }
        protected override CommandInfo FindSpotOnStep(StepInfo cur, string printDebug = "")
        {
            if (cur.name == failStep) return null;
            return new CommandInfo();
        }
    }
    [TestClass]
    public class vcontrolTests
    {
        [TestMethod]
        public void TestGoodMatch()
        {
            FakeProcessingContext pc = new FakeProcessingContext();
            TestSwtchAct ts = new TestSwtchAct(pc, null);
            var res = ts.Process();
            Assert.IsTrue(res.finished);
            var goodSteps = TestSwtchAct.switchSteps;
            for (int i = 0; i < goodSteps.Count; i++)
            {
                var tmpl = goodSteps[i];
                Assert.AreEqual(0, pc.mouseMoveTos[i].x);
                Assert.AreEqual(0, pc.mouseMoveTos[i].y);
                Assert.AreEqual(tmpl.xoff, pc.mouseMoveAndClick[i].x);
                Assert.AreEqual(tmpl.yoff, pc.mouseMoveAndClick[i].y);
            }
        }

        [TestMethod]
        public void TestDisconnected()
        {
            FakeProcessingContext pc = new FakeProcessingContext();
            TestSwtchAct ts = new TestSwtchAct(pc, "FindPlaySignin");
            var res = ts.Process();
            Assert.IsTrue(res.finished);
            var goodSteps = TestSwtchAct.switchSteps;
            Assert.AreEqual(pc.mouseMoveAndClick.Count, goodSteps.Count-1);

            Assert.AreEqual(10, pc.mouseMoveAndClick[0].x);
            Assert.AreEqual(101, pc.mouseMoveAndClick[1].x);
            Assert.AreEqual(107, pc.mouseMoveAndClick[2].x);
        }
    }
}
