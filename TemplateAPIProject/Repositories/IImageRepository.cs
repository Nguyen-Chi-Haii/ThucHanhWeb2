using TemplateAPIProject.Models.Domain;

namespace TemplateAPIProject.Repositories
{
    public interface IImageRepository
    {
        Image Upload(Image image);

        List<Image> GetAll();

        (byte[],string,string) Download(int id);
    }
}
