using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Models;

[Keyless]
public record SearchResultItem(long Id, string Name, double NameSimilarity, double DescriptionSimilarity);

[Keyless]
public record FuzzySearchResultItem(long Id, string Name, double NameSimilarity);
