import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import {
  FaMicrochip, FaTshirt, FaBook, FaHome, FaTag,
  FaDumbbell, FaShoePrints, FaGem, FaGamepad,
  FaHeartbeat, FaUtensils, FaCouch, FaPencilAlt,
  FaCar, FaTools, FaMobileAlt, FaLaptop,
  FaCamera, FaSuitcase, FaRing, FaBaby,
  FaPaw, FaBriefcase, FaHardHat,
} from "react-icons/fa";
import { useProductStore } from "../app/productStore";
import ProductCard from "../components/product/ProductCard";
import Spinner from "../components/ui/Spinner";

const CATEGORY_ICONS = {
  // Seeded categories
  electronics:        FaMicrochip,
  clothing:           FaTshirt,
  books:              FaBook,
  "home & kitchen":   FaHome,
  home:               FaHome,
  kitchen:            FaUtensils,
  health:             FaHeartbeat,
  // Common admin-added categories
  helmets:            FaHardHat,
  sports:             FaDumbbell,
  "sports & fitness": FaDumbbell,
  fitness:            FaDumbbell,
  footwear:           FaShoePrints,
  shoes:              FaShoePrints,
  accessories:        FaGem,
  jewellery:          FaRing,
  jewelry:            FaRing,
  toys:               FaGamepad,
  "toys & games":     FaGamepad,
  beauty:             FaHeartbeat,
  "health & beauty":  FaHeartbeat,
  "personal care":    FaHeartbeat,
  food:               FaUtensils,
  grocery:            FaUtensils,
  groceries:          FaUtensils,
  furniture:          FaCouch,
  stationery:         FaPencilAlt,
  "office supplies":  FaPencilAlt,
  automotive:         FaCar,
  vehicles:           FaCar,
  tools:              FaTools,
  hardware:           FaTools,
  mobile:             FaMobileAlt,
  phones:             FaMobileAlt,
  smartphones:        FaMobileAlt,
  laptops:            FaLaptop,
  computers:          FaLaptop,
  cameras:            FaCamera,
  photography:        FaCamera,
  bags:               FaSuitcase,
  luggage:            FaSuitcase,
  travel:             FaSuitcase,
  baby:               FaBaby,
  kids:               FaBaby,
  children:           FaBaby,
  pets:               FaPaw,
  "pet supplies":     FaPaw,
  office:             FaBriefcase,
};

function getCategoryIcon(name) {
  return CATEGORY_ICONS[name?.toLowerCase()] ?? FaTag;
}

// All slides use bg-[#010101] to match navbar brand-primary exactly.
// Only blobs change — one is always brand purple for consistency.
const SLIDES_THEME = [
  {
    blob1: "bg-[#7f5af0]/40",
    blob2: "bg-indigo-600/30",
    blob3: "bg-fuchsia-600/20",
    ring: "bg-[#7f5af0]/10 border-[#7f5af0]/20",
    letter: "text-[#7f5af0]",
  },
  {
    blob1: "bg-[#7f5af0]/35",
    blob2: "bg-[#2cb67d]/30",
    blob3: "bg-teal-500/20",
    ring: "bg-[#2cb67d]/10 border-[#2cb67d]/20",
    letter: "text-[#2cb67d]",
  },
  {
    blob1: "bg-[#7f5af0]/40",
    blob2: "bg-blue-600/30",
    blob3: "bg-cyan-500/20",
    ring: "bg-blue-500/10 border-blue-500/20",
    letter: "text-blue-300",
  },
  {
    blob1: "bg-[#7f5af0]/35",
    blob2: "bg-rose-600/30",
    blob3: "bg-pink-500/20",
    ring: "bg-rose-500/10 border-rose-500/20",
    letter: "text-rose-300",
  },
  {
    blob1: "bg-[#7f5af0]/40",
    blob2: "bg-amber-500/25",
    blob3: "bg-orange-500/20",
    ring: "bg-amber-500/10 border-amber-500/20",
    letter: "text-amber-300",
  },
];

const TAGLINES = [
  "Premium quality, built to last.",
  "Performance meets precision.",
  "Engineered for everyday use.",
  "Style and substance, combined.",
  "Built for those who demand more.",
];

const heroItemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: (i) => ({
    opacity: 1,
    y: 0,
    transition: { delay: i * 0.15, duration: 0.4, ease: "easeOut" },
  }),
  exit: { opacity: 0, y: -8, transition: { duration: 0.2 } },
};

const categoryContainerVariants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.08 } },
};

const categoryItemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.35, ease: "easeOut" } },
};

export default function HomePage() {
  const { featured, categories, loading, fetchFeatured, fetchCategories } =
    useProductStore();

  const [currentSlide, setCurrentSlide] = useState(0);
  const [paused, setPaused] = useState(false);

  useEffect(() => {
    fetchFeatured();
    fetchCategories();
  }, [fetchFeatured, fetchCategories]);

  useEffect(() => {
    if (categories.length === 0 || paused) return;
    const timer = setInterval(() => {
      setCurrentSlide((prev) => (prev + 1) % categories.length);
    }, 4000);
    return () => clearInterval(timer);
  }, [categories.length, paused, currentSlide]);

  const slide = categories[currentSlide];
  const theme = SLIDES_THEME[currentSlide % SLIDES_THEME.length];
  const tagline = TAGLINES[currentSlide % TAGLINES.length];

  return (
    <div>
      {/* Hero Slider */}
      <section
        className="relative overflow-hidden bg-brand-primary"
        onMouseEnter={() => setPaused(true)}
        onMouseLeave={() => setPaused(false)}
      >
        {/* Mesh Gradient Blobs */}
        <div className={`absolute -top-24 right-0 w-[500px] h-[500px] ${theme.blob1} rounded-full blur-[130px] transition-all duration-700`} />
        <div className={`absolute bottom-0 -left-24 w-[400px] h-[400px] ${theme.blob2} rounded-full blur-[110px] transition-all duration-700`} />
        <div className={`absolute top-1/2 left-1/3 w-[300px] h-[300px] ${theme.blob3} rounded-full blur-[90px] transition-all duration-700`} />

        <div className="relative z-10 max-w-7xl mx-auto px-6 lg:px-8 py-20 flex flex-col md:flex-row items-center gap-12 min-h-[380px]">

          {/* Left — Category Visual */}
          <div className="shrink-0 flex items-center justify-center">
            <div className={`w-48 h-48 rounded-3xl ${theme.ring} border backdrop-blur-sm flex items-center justify-center shadow-2xl`}>
              {slide ? (() => {
                const Icon = getCategoryIcon(slide.name);
                return <Icon className={`w-20 h-20 ${theme.letter}`} />;
              })() : (
                <Spinner />
              )}
            </div>
          </div>

          {/* Right — Staggered content, re-animates on each slide change */}
          <AnimatePresence mode="wait">
            <motion.div
              key={currentSlide}
              className="flex flex-col gap-5 text-white"
            >
              <motion.span
                custom={0}
                variants={heroItemVariants}
                initial="hidden"
                animate="visible"
                exit="exit"
                className="text-xs font-bold uppercase tracking-[0.2em] text-white/40"
              >
                Featured Category
              </motion.span>

              <motion.h1
                custom={1}
                variants={heroItemVariants}
                initial="hidden"
                animate="visible"
                exit="exit"
                className="text-4xl sm:text-5xl font-black leading-tight"
              >
                {slide?.name ?? "Loading..."}
              </motion.h1>

              <motion.p
                custom={2}
                variants={heroItemVariants}
                initial="hidden"
                animate="visible"
                exit="exit"
                className="text-white/60 text-lg max-w-md leading-relaxed"
              >
                {tagline}
              </motion.p>

              {slide && (
                <motion.div
                  custom={3}
                  variants={heroItemVariants}
                  initial="hidden"
                  animate="visible"
                  exit="exit"
                >
                  <Link
                    to={`/shop?category=${encodeURIComponent(slide.name)}`}
                    className="inline-block bg-action-main text-white font-bold px-8 py-3 rounded-full hover:bg-action-hover transition-colors"
                  >
                    Shop {slide.name}
                  </Link>
                </motion.div>
              )}
            </motion.div>
          </AnimatePresence>
        </div>

        {/* Navigation Dots */}
        {categories.length > 1 && (
          <div className="absolute bottom-5 left-1/2 -translate-x-1/2 flex items-center gap-2 z-10">
            {categories.map((_, i) => (
              <button
                key={i}
                onClick={() => setCurrentSlide(i)}
                className={`rounded-full transition-all duration-300 ${
                  i === currentSlide
                    ? "w-6 h-2 bg-action-main"
                    : "w-2 h-2 bg-white/20 hover:bg-white/40"
                }`}
                aria-label={`Go to slide ${i + 1}`}
              />
            ))}
          </div>
        )}
      </section>

      {/* Categories — staggered fade-in on mount */}
      {categories.length > 0 && (
        <section className="max-w-7xl mx-auto px-4 py-12">
          <h2 className="text-xl font-bold text-gray-900 mb-6">Shop by Category</h2>
          <motion.div
            className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4"
            variants={categoryContainerVariants}
            initial="hidden"
            animate="visible"
          >
            {categories.map((cat) => (
              <motion.div key={cat.id} variants={categoryItemVariants}>
                <Link
                  to={`/shop?category=${encodeURIComponent(cat.name)}`}
                  className="flex flex-col items-center gap-2 p-4 bg-white rounded-xl border border-gray-100 hover:shadow-md hover:border-action-main/30 transition-all"
                >
                  {(() => {
                    const Icon = getCategoryIcon(cat.name);
                    return (
                      <div className="w-12 h-12 bg-action-main/10 rounded-full flex items-center justify-center text-action-main">
                        <Icon className="w-5 h-5" />
                      </div>
                    );
                  })()}
                  <span className="text-xs font-medium text-gray-700 text-center line-clamp-2">
                    {cat.name}
                  </span>
                </Link>
              </motion.div>
            ))}
          </motion.div>
        </section>
      )}

      {/* Featured Products — scroll-triggered reveal per card */}
      <section className="max-w-7xl mx-auto px-4 py-8 pb-16">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold text-gray-900">Featured Products</h2>
          <Link to="/shop" className="text-sm font-medium text-action-main hover:text-action-hover">
            View all
          </Link>
        </div>
        {loading ? (
          <Spinner />
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
            {featured.map((p, index) => (
              <motion.div
                key={p.id}
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true, amount: 0.1 }}
                transition={{ duration: 0.4, delay: index * 0.07, ease: "easeOut" }}
              >
                <ProductCard product={p} />
              </motion.div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
