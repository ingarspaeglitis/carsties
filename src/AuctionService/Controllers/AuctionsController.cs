using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController(AuctionDbContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
    {
        var auctions = await context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();

        return Ok(mapper.Map<List<AuctionDto>>(auctions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
    {
        var auction = await context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction([FromBody] CreateAuctionDto auctionDto)
    {
        var auction = mapper.Map<Auction>(auctionDto);

        // TODO: add current user as seller
        auction.Seller = "test";

        context.Auctions.Add(auction);
        var result = await context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Failed to create auction");
        }

        return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto auctionDto)
    {
        var auction = await context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        // TODO: check seller is the same as current user

        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = auctionDto.Year;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage;

        var result = await context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Failed to update auction");
        }

        return Ok(mapper.Map<AuctionDto>(auction));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id) 
    {
        var auction = await context.Auctions.FindAsync(id);
        if (auction == null)
        {
            return NotFound();
        }

         // TODO: check seller is the same as current user
        context.Auctions.Remove(auction);
        var result = await context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Failed to delete auction");
        }
        return Ok();
    } 
}
