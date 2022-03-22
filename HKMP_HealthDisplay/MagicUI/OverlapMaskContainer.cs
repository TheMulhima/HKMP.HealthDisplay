namespace HKMP_HealthDisplay.UI;

public class OverlapMaskContainer : Container
    {
        public OverlapMaskContainer(LayoutRoot onLayout, string name = "New Health display") : base(onLayout, name)
        { }
        protected override Vector2 MeasureOverride()
        {
            // I am exactly half as big as my child is to cause overlap
            var measure = Child.Measure();
            return new Vector2(measure.x/2, measure.y);
        }

        protected override void ArrangeOverride(Vector2 alignedTopLeftCorner)
        {
            // arrange my child with its desired size in the position allocated to me
            Child?.Arrange(new Rect(alignedTopLeftCorner, Child.EffectiveSize));
        }

        protected override void DestroyOverride()
        {
            Child?.Destroy();
            Child = null;
        }
    }