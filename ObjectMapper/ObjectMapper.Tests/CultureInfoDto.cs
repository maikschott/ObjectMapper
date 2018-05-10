using System;
using System.Collections.Generic;

namespace Masch.ObjectMapper.Tests
{
  public class CultureInfoDto
  {
    private NumberFormatResult numericDefault;

    public CultureInfoDto()
    {
      NumericGetOnly = new NumberFormatResult();
      NumericDefault = new NumberFormatResult();
      Percent = new PercentResult();
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string LocalName { get; set; }

    public string Calendar { get; set; }

    public int CultureTypes { get; set; }

    public NumberFormatResult NumericGetOnly { get; }

    public NumberFormatResult NumericNull { get; set; }

    public NumberFormatResult NumericDefault
    {
      get { return numericDefault; }
      set
      {
        if (numericDefault == value) { return; }
        numericDefault = value;
        NumericDefaultSetterCount++;
      }
    }

    public int NumericDefaultSetterCount { get; set; }

    public PercentResult Percent { get; set; }
  }

  public class NumberFormatResult
  {
    public IEnumerable<string> Digits { get; set; }

    public PercentResult Percent { get; set; }
  }

  public struct PercentResult
  {
    public string Sign { get; set; }
  }
}