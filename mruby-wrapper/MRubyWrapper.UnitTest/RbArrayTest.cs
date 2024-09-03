namespace MRubyWrapper.UnitTest;

using Library;
using Library.Language;

public class RbArrayTests : IDisposable
{
    private readonly RbState state;

    public RbArrayTests()
    {
        this.state = Ruby.Open();
    }

    public void Dispose()
    {
        Ruby.Close(this.state);
    }

    [Fact]
    public void TestCreateEmptyArray()
    {
        var array = this.state.NewArray();
        Assert.Equal(this.state, array.RbState);
        Assert.Equal(0, array.Length);
    }

    [Fact]
    public void TestCreateArrayFromValues()
    {
        var values = new List<RbValue>
        {
            this.state.BoxInt(1),
            this.state.BoxInt(2),
        };
        var array = this.state.NewArray(values);
        Assert.Equal(this.state, array.RbState);
        Assert.Equal(2, array.Length);
        Assert.Equal(1, this.state.UnboxInt(array[0]));
        Assert.Equal(2, this.state.UnboxInt(array[1]));
    }

    [Fact]
    public void TestCreateAssocArray()
    {
        var car = this.state.BoxInt(1);
        var cdr = this.state.BoxInt(2);
        var array = RbArray.AssocNew(this.state, car, cdr);

        Assert.Equal(this.state, array.RbState);
        Assert.Equal(2, array.Length);
        Assert.Equal(1, this.state.UnboxInt(array[0]));
        Assert.Equal(2, this.state.UnboxInt(array[1]));
    }

    [Fact]
    public void TestConcatenateArrays()
    {
        var values0 = new List<RbValue>
        {
            this.state.BoxInt(1),
            this.state.BoxInt(2),
        };

        var values1 = new List<RbValue>
        {
            this.state.BoxInt(3),
            this.state.BoxInt(4),
        };

        var array1 = this.state.NewArray(values0);
        var array2 = this.state.NewArray(values1);
        array1.Concat(array2);

        Assert.Equal(4, array1.Length);
        Assert.Equal(1, this.state.UnboxInt(array1[0]));
        Assert.Equal(2, this.state.UnboxInt(array1[1]));
        Assert.Equal(3, this.state.UnboxInt(array1[2]));
        Assert.Equal(4, this.state.UnboxInt(array1[3]));
    }

    [Fact]
    public void TestAddValueToArray()
    {
        var array = this.state.NewArray();
        var value = this.state.BoxInt(1);
        array.Push(value);

        Assert.Equal(1, array.Length);
        Assert.Equal(1, this.state.UnboxInt(array[0]));
    }

    [Fact]
    public void TestRemoveAndReturnLastValue()
    {
        var array = this.state.NewArray();
        var value = this.state.BoxInt(1);

        array.Push(value);
        var poppedValue = array.Pop();

        Assert.Equal(value, poppedValue);
        Assert.Equal(0, array.Length);
    }

    [Fact]
    public void TestSetValueAtIndex()
    {
        var array = this.state.NewArray();
        var value = this.state.BoxInt(1);
        array[0] = value;

        Assert.Equal(1, array.Length);
        Assert.Equal(1, this.state.UnboxInt(array[0]));
    }

    [Fact]
    public void TestReplaceArrayContents()
    {
        var values0 = new List<RbValue>
        {
            this.state.BoxInt(1),
            this.state.BoxInt(2),
        };

        var values1 = new List<RbValue>
        {
            this.state.BoxInt(3),
            this.state.BoxInt(4),
        };

        var array1 = this.state.NewArray(values0);
        var array2 = this.state.NewArray(values1);

        array1.Replace(array2);

        Assert.Equal(2, array1.Length);
        Assert.Equal(3, this.state.UnboxInt(array1[0]));
        Assert.Equal(4, this.state.UnboxInt(array1[1]));
    }

    [Fact]
    public void TestUnshiftToAddValueToFront()
    {
        var values0 = new List<RbValue>
        {
            this.state.BoxInt(1),
            this.state.BoxInt(2),
        };

        var array = this.state.NewArray(values0);
        var value = this.state.BoxInt(3);
        array.Unshift(value);

        Assert.Equal(3, array.Length);
        Assert.Equal(3, this.state.UnboxInt(array[0]));
    }

    [Fact]
    public void TestGetReturnValueAtIndex()
    {
        var array = this.state.NewArray();
        var value = this.state.BoxInt(1);
        array.Push(value);
        var retrievedValue = array.Get(0);

        Assert.Equal(1, array.Length);
        Assert.Equal(value, retrievedValue);
    }

    [Fact]
    public void TestSpliceReplacePartOfArray()
    {
        var values0 = new RbValue[]
        {
            this.state.BoxInt(1),
            this.state.BoxInt(2),
            this.state.BoxInt(3),
            this.state.BoxInt(4),
            this.state.BoxInt(5),
        };

        var values1 = new RbValue[]
        {
            this.state.BoxInt(6),
            this.state.BoxInt(7),
        };

        var array0 = this.state.NewArray(values0);
        var array1 = this.state.NewArray(values1);

        var array2 = array0.Splice(1, 2);
        Assert.Equal(3, array2.Length);
        Assert.Equal(1, this.state.UnboxInt(array2[0]));
        Assert.Equal(4, this.state.UnboxInt(array2[1]));
        Assert.Equal(5, this.state.UnboxInt(array2[2]));

        var array3 = array0.Splice(1, 1, array1);
        Assert.Equal(4, array3.Length);
        Assert.Equal(1, this.state.UnboxInt(array3[0]));
        Assert.Equal(6, this.state.UnboxInt(array3[1]));
        Assert.Equal(7, this.state.UnboxInt(array3[2]));
        Assert.Equal(5, this.state.UnboxInt(array3[3]));
    }

    [Fact]
    public void TestShiftRemoveAndReturnFirstValue()
    {
        var array = this.state.NewArray();
        var value = this.state.BoxInt(1);
        array.Push(value);
        var shiftedValue = array.Shift();

        Assert.Equal(0, array.Length);
        Assert.Equal(1, this.state.UnboxInt(shiftedValue));
        Assert.Equal(value, shiftedValue);
    }

    [Fact]
    public void TestClearRemoveAllValues()
    {
        var values0 = new RbValue[]
        {
            this.state.BoxInt(1),
            this.state.BoxInt(2),
            this.state.BoxInt(3),
            this.state.BoxInt(4),
            this.state.BoxInt(5),
        };
        
        var array = this.state.NewArray(values0);
        array.Clear();

        Assert.Equal(0, array.Length);
    }

    [Fact]
    public void TestJoinArrayValues()
    {
        var values0 = new RbValue[]
        {
            this.state.BoxString("1"),
            this.state.BoxString("2"),
            this.state.BoxString("3"),
            this.state.BoxString("4"),
            this.state.BoxString("5"),
        };

        var array = this.state.NewArray(values0);
        var sep = this.state.BoxString(",");
        var joinedValue = array.Join(sep);

        var unboxedJoint = this.state.UnboxString(joinedValue);
        
        Assert.Equal("1,2,3,4,5", unboxedJoint);
    }

    [Fact]
    public void TestResizeArray()
    {
        var array = this.state.NewArray();
        array.Resize(10);

        Assert.Equal(10, array.Length);
    }
}