using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApi.Data;

namespace BlogApi.Controllers
{
    [Route("api/address")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public AddressController(BlogDbContext context)
        {
            _context = context;
        }

    [HttpGet("search")]
        public async Task<IActionResult> SearchAddress([FromQuery] long? objectid, [FromQuery] string? query)
        {
            IQueryable<long> objectIdsQuery;
            if (objectid.HasValue)
            {
                objectIdsQuery = _context.AdmHierarchies
                    .Where(h => h.parentobjid == objectid && h.isactive == 1)
                    .Select(h => h.objectid);
            }
            else
            {
                objectIdsQuery = _context.AdmHierarchies
                    .Where(h => h.parentobjid == 0 && h.isactive == 1)
                    .Select(h => h.objectid);
            }
            var objectIds = await objectIdsQuery.ToListAsync();
            var addrQuery = _context.AddrObjs
                .Where(a => objectIds.Contains(a.objectid) && a.isactive == 1)
                .Select(a => new
                {
                    a.objectid,
                    a.objectguid,
                    text = a.typename + " " + a.name,
                    level = a.level
                });

            var houseQuery = _context.Houses
                .Where(h => objectIds.Contains(h.objectid) && h.isactive == 1)
                .Select(h => new
                {
                    h.objectid,
                    h.objectguid,
                    text = h.housenum,
                    level = "Здание (сооружение)"
                });
            var combinedQuery = addrQuery.Cast<object>().Concat(houseQuery.Cast<object>());
            if (!string.IsNullOrEmpty(query))
            {
                combinedQuery = combinedQuery
                    .Where(item =>
                        EF.Functions.ILike(EF.Property<string>(item, "text"), $"%{query}%"));
            }
            var resultQuery = from item in combinedQuery
                            join level in _context.ObjectLevels
                            on EF.Property<string>(item, "level") equals level.level.ToString() into levels
                            from level in levels.DefaultIfEmpty()
                            select new
                            {
                                objectid = EF.Property<long>(item, "objectid"),
                                objectguid = EF.Property<Guid>(item, "objectguid"),
                                text = EF.Property<string>(item, "text"),
                                objectlevel = level != null ? level.name : EF.Property<string>(item, "level")
                            };
            var result = await resultQuery
                                    .GroupBy(r => r.objectid)
                                    .Select(g => g.FirstOrDefault())
                                    .ToListAsync();
            return Ok(result);
        }
        [HttpGet("chain")]
        public async Task<IActionResult> GetAddressChain([FromQuery] Guid objectguid)
        {
            var addrObj = await _context.AddrObjs
                .Where(a => a.objectguid == objectguid && a.isactive == 1)
                .FirstOrDefaultAsync();

            var house = await _context.Houses
                .Where(h => h.objectguid == objectguid && h.isactive == 1)
                .FirstOrDefaultAsync();
            if (addrObj == null && house == null)
            {
                return NotFound("Address object not found.");
            }
            var objectid = addrObj?.objectid ?? house?.objectid;
            var objectguidFound = addrObj?.objectguid ?? house?.objectguid;
            var hierarchy = await _context.AdmHierarchies
                .Where(h => h.objectid == objectid && h.isactive == 1)
                .FirstOrDefaultAsync();

            if (hierarchy == null)
            {
                return NotFound("Hierarchy entry not found.");
            }
            var path = hierarchy.path;
            var objectIds = path.Split('.').Select(long.Parse).ToList();
            var addrQuery = _context.AddrObjs
                .Where(a => objectIds.Contains(a.objectid) && a.isactive == 1)
                .Select(a => new
                {
                    a.objectid,
                    a.objectguid,
                    text = a.typename + " " + a.name,
                    objectlevel = a.level,
                });
            var houseQuery = _context.Houses
                .Where(h => objectIds.Contains(h.objectid) && h.isactive == 1)
                .Select(h => new
                {
                    h.objectid,
                    h.objectguid,
                    text = h.housenum,
                    objectlevel = "Здание (Сооружение)",
                });
            var combinedQuery = addrQuery.Cast<object>().Concat(houseQuery.Cast<object>());
            var resultQuery = from item in combinedQuery
                            join level in _context.ObjectLevels
                            on EF.Property<string>(item, "objectlevel") equals level.level.ToString() into levels
                            from level in levels.DefaultIfEmpty()
                            select new
                            {
                                objectid = EF.Property<long>(item, "objectid"),
                                objectguid = EF.Property<Guid>(item, "objectguid"),
                                text = EF.Property<string>(item, "text"),
                                objectlevel = level != null ? level.name : EF.Property<string>(item, "objectlevel"),
                            };
            var orderedResult = resultQuery
                .OrderBy(item => objectIds.IndexOf(item.objectid))
                .ToList();
            return Ok(orderedResult);
        }
    }
}
