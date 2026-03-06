using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using MVCAllOptions.Books;

namespace MVCAllOptions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MVCAllOptionsBookToBookDtoMapper : MapperBase<Book, BookDto>
{
    public override partial BookDto Map(Book source);

    public override partial void Map(Book source, BookDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class MVCAllOptionsCreateUpdateBookDtoToBookMapper : MapperBase<CreateUpdateBookDto, Book>
{
    public override partial Book Map(CreateUpdateBookDto source);

    public override partial void Map(CreateUpdateBookDto source, Book destination);
}
