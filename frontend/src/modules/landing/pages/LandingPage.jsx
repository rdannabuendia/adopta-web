import Navbar from "../components/Navbar/Navbar";
import Hero from "../components/Hero/Hero";
import "../../../styles/landing.css";
import About from "../components/About/About";
import HowItWorks from "../components/HowItWorks/HowItWorks";
import Faq from "../components/Faq/Faq";
import Contact from "../components/Contact/Contact";
import Footer from "../components/Footer/Footer";

export default function LandingPage() {
  return (
    <div className="landing">
      <Navbar/>
      <Hero/>
      <About/>
      <HowItWorks/>
      <Faq/>
      <Contact/>
      <Footer/>
    </div>
  );
}