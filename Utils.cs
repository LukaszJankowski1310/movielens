
namespace web_services_l1 {
public class Utils {

   public static float parseFloat(string s) {
        float f = 0;
        string s1 = s[0].ToString();
        f += float.Parse(s1);
        if (s[2] == '5') {
            f+= (float) 0.5;
        }

        return f;
    }

}
}