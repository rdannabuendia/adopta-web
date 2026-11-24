import Navbar from "../components/Navbar/Navbar";
import Hero from "../components/Hero/Hero";
import "../../../styles/landing.css";
import About from "../components/About/About";
import HowItWorks from "../components/HowItWorks/HowItWorks";
import Faq from "../components/Faq/Faq";
import Contact from "../components/Contact/Contact";
import Footer from "../components/Footer/Footer";
import PetsCarousel from "../components/PetsCarousel/PetsCarousel"
import Platforms from "../components/Platforms/Platforms"

export default function LandingPage() {
  return (
    <div className="landing">
      <Navbar/>
      <Hero/>
      <PetsCarousel />
      <About />
      <HowItWorks />
      <Platforms />
      <Faq />
      <Contact />
      <Footer />
    </div>
  );
}
