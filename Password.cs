using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace PasswordGenerator
{
    public class Password
    {

        [Required]
         public string Name {get; set ;}

        [Required]
         public string Hash {get; set;}
        public Password(string Pname , string load){
            
            Name = Pname;
            Hash = load;

        }
    }

}
