using HF.Application.AssetsLiabilities.Requests;
using HF.Application.AssetsLiabilities.Services;
using HF.Application.Tests.Common;
using HF.Domain.AssetsLiabilities;
using Xunit;

namespace HF.Application.Tests.AssetsLiabilities;

public class AssetsLiabilitiesServiceTests
{
    [Fact]
    public async Task AssetService_CreateAndGet_ShouldSucceed()
    {
        var repo = new InMemoryRepository<Asset>();
        var sut = new AssetService(repo);

        var request = new CreateAssetRequest(
            Name: "Computer Lab Desktop",
            AssetTag: "ASSET-2026-001",
            AcquisitionValue: 3500.00m,
            Category: AssetCategory.ComputerEquipment);

        var created = await sut.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal("ASSET-2026-001", created.AssetTag);
        Assert.Equal(3500.00m, created.AcquisitionValue);

        var fetched = await sut.GetByIdAsync(created.Id, CancellationToken.None);
        Assert.NotNull(fetched);
        Assert.Equal("Computer Lab Desktop", fetched.Name);
    }

    [Fact]
    public async Task LiabilityService_CreateAndList_ShouldSucceed()
    {
        var repo = new InMemoryRepository<Liability>();
        var sut = new LiabilityService(repo);

        var request = new CreateLiabilityRequest(
            Title: "Internet Connectivity Nov 2026",
            Amount: 250.00m,
            DueDate: new DateOnly(2026, 11, 20),
            Creditor: "Telecom Provider",
            Category: LiabilityCategory.Utilities);

        var created = await sut.CreateAsync(request, CancellationToken.None);
        Assert.NotNull(created);

        var list = await sut.ListAsync(new ListLiabilitiesRequest(), CancellationToken.None);
        Assert.Single(list);
        Assert.Equal(250.00m, list[0].Amount);
    }
}

