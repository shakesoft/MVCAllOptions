using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using MVCAllOptions.Books;

namespace MVCAllOptions.Web;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MVCAllOptionsWebMappers : MapperBase<BookDto, CreateUpdateBookDto>
{
    public override partial CreateUpdateBookDto Map(BookDto source);

    public override partial void Map(BookDto source, CreateUpdateBookDto destination);
}
