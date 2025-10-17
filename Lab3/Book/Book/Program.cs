// top lev prog
Console.WriteLine("~~~ CQRS Library Example ~~~");

Middleware.Handle(() =>
{
    // books for demo
    BookHandler.Create(new Book(1, "Crime and Punishment", "Fyodor Dostoevsky", 1866));
    BookHandler.Create(new Book(2, "The Idiot", "Fyodor Dostoevsky", 1869));
    BookHandler.Create(new Book(3, "The Brothers Karamazov", "Fyodor Dostoevsky", 1880));

    BookHandler.Create(new Book(4, "War and Peace", "Leo Tolstoy", 1869));
    BookHandler.Create(new Book(5, "Anna Karenina", "Leo Tolstoy", 1877));

    BookHandler.Create(new Book(6, "Pride and Prejudice", "Jane Austen", 1813));
    BookHandler.Create(new Book(7, "Sense and Sensibility", "Jane Austen", 1811));
    BookHandler.Create(new Book(8, "Emma", "Jane Austen", 1815));

    BookHandler.Create(new Book(9, "Great Expectations", "Charles Dickens", 1861));
    BookHandler.Create(new Book(11, "Oliver Twist", "Charles Dickens", 1838));
});

while (true)
{
    Console.WriteLine("\nType a command to interact with the library:");
    Console.WriteLine("Available commands: list | filter | sort | page | create | update | delete | exit");
    Console.Write(">>> ");
    string cmd = Console.ReadLine()?.ToLower() ?? "";

    if (cmd == "exit")
        break;

    switch (cmd)
    {
        case "list":
            foreach (var b in BookHandler.GetAll())
                Console.WriteLine($"{b.Id}: {b.Title} - {b.Author} ({b.Year})");
            break;

        case "filter":
            Console.Write("Autor: ");
            string author = Console.ReadLine()!;
            var filtered = BookHandler.FilterByAuthor(author);
            foreach (var b in filtered)
                Console.WriteLine($"{b.Title} ({b.Year})");
            if (!filtered.Any()) Console.WriteLine("No book found for this author.");
            break;

        case "sort":
            Console.Write("Criteria (title/year): ");
            string crit = Console.ReadLine()!;
            foreach (var b in BookHandler.Sort(crit))
                Console.WriteLine($"{b.Title} ({b.Year})");
            break;

        case "page":
            Console.Write("Page: ");
            int p = int.Parse(Console.ReadLine()!);
            Console.Write("Page size: ");
            int s = int.Parse(Console.ReadLine()!);
            foreach (var b in BookHandler.GetPage(p, s))
                Console.WriteLine($"{b.Title} ({b.Year})");
            break;

        case "create":
            Console.Write("ID: ");
            int id = int.Parse(Console.ReadLine()!);
            Console.Write("Title: ");
            string title = Console.ReadLine()!;
            Console.Write("Author: ");
            string a = Console.ReadLine()!;
            Console.Write("Year published: ");
            int y = int.Parse(Console.ReadLine()!);
            Middleware.Handle(() => BookHandler.Create(new Book(id, title, a, y)));
            break;

        case "update":
            Console.Write("ID: ");
            int idU = int.Parse(Console.ReadLine()!);
            Console.Write("New title: ");
            string newTitle = Console.ReadLine()!;
            Middleware.Handle(() => BookHandler.Update(idU, newTitle));
            break;

        case "delete":
            Console.Write("ID: ");
            int idD = int.Parse(Console.ReadLine()!);
            Middleware.Handle(() => BookHandler.Delete(idD));
            break;

        default:
            Console.WriteLine("Unknown command!");
            break;
    }
}

//entity
public record Book(int Id, string Title, string Author, int Year);

//exceptions
public class BookNotFoundException : Exception
{
    public BookNotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

//middleware
public static class Middleware
{
    public static void Handle(Action action)
    {
        try { action(); }
        catch (ValidationException ex) { Console.WriteLine($"[Validation Error] {ex.Message}"); }
        catch (BookNotFoundException ex) { Console.WriteLine($"[Book Error] {ex.Message}"); }
        catch (Exception ex) { Console.WriteLine($"[General Error] {ex.Message}"); }
    }
}

//handler cqrs
public static class BookHandler
{
    private static readonly List<Book> books = new();

    // create
    public static void Create(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            throw new ValidationException("Title and author fields cannot be empty!");
        if (books.Any(b => b.Id == book.Id))
            throw new ValidationException($"There already is a book with ID {book.Id}!");
        books.Add(book);
    }

    // read all
    public static IEnumerable<Book> GetAll() => books;

    // filter(author)
    public static IEnumerable<Book> FilterByAuthor(string author) =>
        books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

    // sort
    public static IEnumerable<Book> Sort(string criteria) => criteria.ToLower() switch
    {
        "title" => books.OrderBy(b => b.Title),
        "year" => books.OrderBy(b => b.Year),
        _ => books
    };

    // update
    public static void Update(int id, string newTitle)
    {
        var book = books.FirstOrDefault(b => b.Id == id)
                   ?? throw new BookNotFoundException($"The book with ID {id} could not be found!");
        var updated = book with { Title = newTitle };
        books[books.IndexOf(book)] = updated;
        Console.WriteLine("Book successfully updated!.");
    }

    // delete
    public static void Delete(int id)
    {
        var book = books.FirstOrDefault(b => b.Id == id)
                   ?? throw new BookNotFoundException($"The book with ID {id} could not be found!");
        books.Remove(book);
        Console.WriteLine("Book successfully deleted!.");
    }

    // pagination
    public static IEnumerable<Book> GetPage(int page, int size) =>
        books.Skip((page - 1) * size).Take(size);
}
